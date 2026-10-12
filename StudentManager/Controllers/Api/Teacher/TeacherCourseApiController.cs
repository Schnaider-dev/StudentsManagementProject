using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Controllers.Api.Teacher;

[ApiController]
[Route("api/teacher/courses")]
[Authorize(Roles = "Teacher")]
public class TeacherCourseApiController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherCourseApiController(
        ICourseService courseService,
        UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _userManager = userManager;
    }

    [HttpGet]
    [EndpointSummary("List the signed-in teacher's active courses")]
    [ProducesResponseType<IReadOnlyList<TeacherCourseResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherCourseResponse>>> GetCourses(
        CancellationToken cancellationToken = default)
    {
        var teacherId = GetTeacherId();
        if (teacherId is null)
        {
            return Unauthorized();
        }

        var courses = await _courseService.GetTeacherCoursesAsync(
            teacherId,
            cancellationToken: cancellationToken);

        var responses = courses
            .Select(course => new TeacherCourseResponse(
                course.CourseId,
                course.Code,
                course.Name,
                new DateTimeOffset(DateTime.SpecifyKind(
                    course.CreatedAt,
                    DateTimeKind.Utc)),
                course.IsActive,
                course.Enrollments.Count,
                course.Assignments.Count))
            .ToList();

        return Ok(responses);
    }

    [HttpGet("{courseId:int}")]
    [EndpointSummary("Get details for one of the signed-in teacher's courses")]
    [ProducesResponseType<TeacherCourseDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherCourseDetailsResponse>> GetCourse(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var teacherId = GetTeacherId();
        if (teacherId is null)
        {
            return Unauthorized();
        }

        var course = await _courseService.GetTeacherCourseAsync(
            courseId,
            teacherId,
            cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        var students = course.Enrollments
            .OrderBy(enrollment => enrollment.Student.LastName)
            .ThenBy(enrollment => enrollment.Student.FirstName)
            .Take(5)
            .Select(enrollment => new CourseStudentResponse(
                enrollment.StudentId,
                enrollment.Student.FirstName,
                enrollment.Student.LastName,
                enrollment.Student.Email))
            .ToList();

        var assignments = course.Assignments
            .Where(assignment => assignment.IsActive)
            .OrderBy(assignment => assignment.DueDate)
            .Select(assignment => new CourseAssignmentResponse(
                assignment.AssignmentId,
                assignment.Title,
                new DateTimeOffset(DateTime.SpecifyKind(
                    assignment.DueDate,
                    DateTimeKind.Utc)),
                assignment.Submissions.Count))
            .ToList();

        var response = new TeacherCourseDetailsResponse(
            course.CourseId,
            course.Code,
            course.Name,
            course.Description,
            new DateTimeOffset(DateTime.SpecifyKind(
                course.CreatedAt,
                DateTimeKind.Utc)),
            course.IsActive,
            course.Enrollments.Count,
            assignments.Count,
            students,
            assignments);

        return Ok(response);
    }

    [HttpPost]
    [EndpointSummary("Create a course for the signed-in teacher")]
    [ProducesResponseType<TeacherCourseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeacherCourseResponse>> CreateCourse(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var teacherId = GetTeacherId();
        if (teacherId is null)
        {
            return Unauthorized();
        }

        try
        {
            var course = await _courseService.CreateCourseAsync(
                new CourseCreateViewModel
                {
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description
                },
                teacherId,
                cancellationToken);

            var response = new TeacherCourseResponse(
                course.CourseId,
                course.Code,
                course.Name,
                new DateTimeOffset(DateTime.SpecifyKind(
                    course.CreatedAt,
                    DateTimeKind.Utc)),
                course.IsActive,
                StudentCount: 0,
                AssignmentCount: 0);

            return CreatedAtAction(
                nameof(GetCourse),
                new { courseId = course.CourseId },
                response);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Course could not be created.",
                Detail = exception.Message
            });
        }
    }

    [HttpPut("{courseId:int}")]
    [EndpointSummary("Update one of the signed-in teacher's courses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(
        int courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var teacherId = GetTeacherId();
        if (teacherId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _courseService.UpdateCourseAsync(
                new CourseEditViewModel
                {
                    CourseId = courseId,
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description
                },
                teacherId,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Course could not be updated.",
                Detail = exception.Message
            });
        }
    }

    [HttpDelete("{courseId:int}")]
    [EndpointSummary("Archive one of the signed-in teacher's courses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveCourse(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var teacherId = GetTeacherId();
        if (teacherId is null)
        {
            return Unauthorized();
        }

        var archived = await _courseService.ArchiveCourseAsync(
            courseId,
            teacherId,
            cancellationToken);

        if (!archived)
        {
            return NotFound();
        }

        return NoContent();
    }

    private string? GetTeacherId()
    {
        return _userManager.GetUserId(User);
    }
}
