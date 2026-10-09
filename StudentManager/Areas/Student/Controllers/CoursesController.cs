using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;

namespace StudentManager.Areas.Student.Controllers;

// These routes belong to the Student area and are restricted to signed-in students.
[Area("Student")]
[Authorize(Roles = "Student")]
public class CoursesController : Controller
{
    // Services keep database and enrollment rules outside the controller.
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    // ASP.NET Core supplies these dependencies when it creates the controller.
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
        // Make the controller's default page the student's list of enrolled courses.
        return RedirectToAction(nameof(MyCourses));
    }

    [HttpGet]
    public async Task<IActionResult> Available(
        int page = 1,
        string? search = null,
        string? filter = null)
    {
        // Read the signed-in student's identity instead of trusting an ID from the URL.
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            // No identity means the request cannot be tied to a student account.
            return Challenge();
        }

        // Ask the service for 10 matching courses and the page metadata for navigation.
        var courses = await _enrollmentService.GetAvailableCoursesAsync(
            studentId,
            page,
            pageSize: 10,
            search,
            filter);

        // Pass the page model to Available.cshtml for display.
        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        // Determine which student's enrollments should be displayed.
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            // Require a valid signed-in student identity.
            return Challenge();
        }

        // Load this student's active course list.
        var courses =
            await _enrollmentService.GetStudentCoursesAsync(
                studentId);

        // Render the list in the MyCourses view.
        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        // Get identity from the authentication system, not from posted form data.
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            // A missing identity cannot create an enrollment.
            return Challenge();
        }

        // The service verifies the course and applies enrollment rules.
        var enrolled = await _enrollmentService.EnrollAsync(
            courseId,
            studentId);

        if (!enrolled)
        {
            // TempData survives the redirect so the next page can display this message.
            TempData["ErrorMessage"] =
                "The course is unavailable or you are already enrolled.";

            // Return to the available list so the student can continue browsing.
            return RedirectToAction(nameof(Available));
        }

        // Show a success message after redirecting to the student's enrolled courses.
        TempData["SuccessMessage"] =
            "You enrolled in the course successfully.";

        return RedirectToAction(nameof(MyCourses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Drop(int courseId)
    {
        // Determine the owner from the signed-in account, never from a client-supplied student ID.
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            // A missing identity cannot drop an enrollment.
            return Challenge();
        }

        // The service only drops an active enrollment matching this student and course.
        var dropped = await _enrollmentService.DropAsync(
            courseId,
            studentId);

        if (!dropped)
        {
            // Tell the next request that there was no matching active enrollment.
            TempData["ErrorMessage"] =
                "The active enrollment was not found.";

            // Return to the course list after handling the unsuccessful request.
            return RedirectToAction(nameof(MyCourses));
        }

        // Report success on the course list after redirecting.
        TempData["SuccessMessage"] =
            "The course was dropped successfully.";

        return RedirectToAction(nameof(MyCourses));
    }

    [HttpGet]
    public async Task<IActionResult> Progress(int courseId)
    {
        // Reject invalid IDs before making a service call.
        if (courseId <= 0)
        {
            return BadRequest();
        }

        // Identify the student whose progress is requested.
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            // Require a signed-in student.
            return Challenge();
        }

        // Load progress only for this student and course.
        var model = await _progressService.GetCourseProgressAsync(
            courseId,
            studentId);

        if (model is null)
        {
            // No matching progress data means the requested resource was not found.
            return NotFound();
        }

        // Render the course-progress details.
        return View(model);
    }

    // UserManager reads the current user's ID from the authenticated request.
    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }
}