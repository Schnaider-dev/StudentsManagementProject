using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;

namespace StudentManager.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class CoursesController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesController(
    IEnrollmentService enrollmentService,
    IProgressService progressService,
    UserManager<ApplicationUser> userManager)
    {
        _enrollmentService = enrollmentService;
        _progressService = progressService;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(MyCourses));
    }

    [HttpGet]
    public async Task<IActionResult> Available()
    {
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var courses =
            await _enrollmentService.GetAvailableCoursesAsync(
                studentId);

        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var courses =
            await _enrollmentService.GetStudentCoursesAsync(
                studentId);

        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var enrolled = await _enrollmentService.EnrollAsync(
            courseId,
            studentId);

        if (!enrolled)
        {
            TempData["ErrorMessage"] =
                "The course is unavailable or you are already enrolled.";

            return RedirectToAction(nameof(Available));
        }

        TempData["SuccessMessage"] =
            "You enrolled in the course successfully.";

        return RedirectToAction(nameof(MyCourses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Drop(int courseId)
    {
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var dropped = await _enrollmentService.DropAsync(
            courseId,
            studentId);

        if (!dropped)
        {
            TempData["ErrorMessage"] =
                "The active enrollment was not found.";

            return RedirectToAction(nameof(MyCourses));
        }

        TempData["SuccessMessage"] =
            "The course was dropped successfully.";

        return RedirectToAction(nameof(MyCourses));
    }

    [HttpGet]
    public async Task<IActionResult> Progress(int courseId)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var model = await _progressService.GetCourseProgressAsync(
            courseId,
            studentId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }
    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }
}