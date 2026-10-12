using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Student;

namespace StudentManager.Controllers.Api.Student;

[ApiController]
[Route("api/student/courses")]
[Authorize(Roles = "Student")]
public class StudentCourseApiController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentCourseApiController(
        IEnrollmentService enrollmentService,
        IProgressService progressService,
        UserManager<ApplicationUser> userManager)
    {
        _enrollmentService = enrollmentService;
        _progressService = progressService;
        _userManager = userManager;
    }

    [HttpGet]
    [EndpointSummary("List courses available for the signed-in student")]
    [ProducesResponseType<AvailableCoursesPageViewModel>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AvailableCoursesPageViewModel>> GetAvailableCourses(
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var studentId = GetStudentId();
        if (studentId is null)
        {
            return Unauthorized();
        }

        var courses = await _enrollmentService.GetAvailableCoursesAsync(
            studentId,
            page,
            pageSize: 10,
            search,
            filter,
            cancellationToken);

        return Ok(courses);
    }

    [HttpGet("enrolled")]
    [EndpointSummary("List courses in which the signed-in student is enrolled")]
    [ProducesResponseType<IReadOnlyList<MyCourseViewModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MyCourseViewModel>>> GetEnrolledCourses(
        CancellationToken cancellationToken = default)
    {
        var studentId = GetStudentId();
        if (studentId is null)
        {
            return Unauthorized();
        }

        var courses = await _enrollmentService.GetStudentCoursesAsync(
            studentId,
            cancellationToken);

        return Ok(courses);
    }

    [HttpGet("{courseId:int}/progress")]
    [EndpointSummary("Get the signed-in student's progress in a course")]
    [ProducesResponseType<CourseProgressViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseProgressViewModel>> GetCourseProgress(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var studentId = GetStudentId();
        if (studentId is null)
        {
            return Unauthorized();
        }

        var progress = await _progressService.GetCourseProgressAsync(
            courseId,
            studentId,
            cancellationToken);

        if (progress is null)
        {
            return NotFound();
        }

        return Ok(progress);
    }

    [HttpPost("{courseId:int}/enrollment")]
    [EndpointSummary("Enroll the signed-in student in a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var studentId = GetStudentId();
        if (studentId is null)
        {
            return Unauthorized();
        }

        var enrolled = await _enrollmentService.EnrollAsync(
            courseId,
            studentId,
            cancellationToken);

        if (!enrolled)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Course enrollment was not completed.",
                Detail = "The course is unavailable or you are already enrolled."
            });
        }

        return NoContent();
    }

    [HttpDelete("{courseId:int}/enrollment")]
    [EndpointSummary("Drop the signed-in student's active course enrollment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Drop(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var studentId = GetStudentId();
        if (studentId is null)
        {
            return Unauthorized();
        }

        var dropped = await _enrollmentService.DropAsync(
            courseId,
            studentId,
            cancellationToken);

        if (!dropped)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Active enrollment not found."
            });
        }

        return NoContent();
    }

    private string? GetStudentId()
    {
        return _userManager.GetUserId(User);
    }
}
