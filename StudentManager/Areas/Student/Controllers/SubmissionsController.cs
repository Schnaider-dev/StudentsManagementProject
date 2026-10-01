using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Student;

namespace StudentManager.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class SubmissionsController : Controller
{
    private readonly ISubmissionService _submissionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubmissionsController(
        ISubmissionService submissionService,
        UserManager<ApplicationUser> userManager)
    {
        _submissionService = submissionService;
        _userManager = userManager;
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

        var submission =
            await _submissionService.GetSubmissionAsync(
                id.Value,
                studentId);

        if (submission is null)
        {
            return NotFound();
        }

        return View(submission);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int assignmentId)
    {
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        var assignment =
            await _submissionService.GetAssignmentAsync(
                assignmentId,
                studentId);

        if (assignment is null)
        {
            return NotFound();
        }

        if (assignment.SubmissionId.HasValue)
        {
            return RedirectToAction(nameof(Details), new
            {
                id = assignment.SubmissionId.Value
            });
        }

        return View(new SubmissionViewModel
        {
            AssignmentId = assignment.AssignmentId,
            AssignmentTitle = assignment.Title
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SubmissionViewModel model)
    {
        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await SetAssignmentTitleAsync(model, studentId);
            return View(model);
        }

        try
        {
            var submission =
                await _submissionService.CreateSubmissionAsync(
                    model,
                    studentId);

            TempData["SuccessMessage"] =
                "Assignment submitted successfully.";

            return RedirectToAction(nameof(Details), new
            {
                id = submission.SubmissionId
            });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await SetAssignmentTitleAsync(model, studentId);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
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

        var submission =
            await _submissionService.GetSubmissionAsync(
                id.Value,
                studentId);

        if (submission is null)
        {
            return NotFound();
        }

        if (submission.Grade is not null)
        {
            TempData["ErrorMessage"] =
                "A graded submission cannot be edited.";

            return RedirectToAction(nameof(Details), new
            {
                id = submission.SubmissionId
            });
        }

        var model = new SubmissionViewModel
        {
            SubmissionId = submission.SubmissionId,
            AssignmentId = submission.AssignmentId,
            AssignmentTitle = submission.Assignment.Title,
            Content = submission.Content
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        SubmissionViewModel model)
    {
        if (!model.SubmissionId.HasValue ||
            id != model.SubmissionId.Value)
        {
            return BadRequest();
        }

        var studentId = GetCurrentUserId();

        if (studentId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await SetAssignmentTitleAsync(model, studentId);
            return View(model);
        }

        try
        {
            var updated =
                await _submissionService.UpdateSubmissionAsync(
                    model,
                    studentId);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Submission updated successfully.";

            return RedirectToAction(nameof(Details), new
            {
                id = model.SubmissionId.Value
            });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await SetAssignmentTitleAsync(model, studentId);
            return View(model);
        }
    }

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }

    private async Task SetAssignmentTitleAsync(
        SubmissionViewModel model,
        string studentId)
    {
        var assignment =
            await _submissionService.GetAssignmentAsync(
                model.AssignmentId,
                studentId);

        model.AssignmentTitle =
            assignment?.Title ?? "Assignment";
    }
}