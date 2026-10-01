using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;

namespace StudentManager.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class AssignmentsController : Controller
{
    private readonly ISubmissionService _submissionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignmentsController(
        ISubmissionService submissionService,
        UserManager<ApplicationUser> userManager)
    {
        _submissionService = submissionService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int courseId)
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

        var model =
            await _submissionService.GetCourseAssignmentsAsync(
                courseId,
                studentId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var model = await _submissionService.GetAssignmentAsync(
            id.Value,
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