using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class GradesController : Controller
{
    private readonly IGradingService _gradingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GradesController(
        IGradingService gradingService,
        UserManager<ApplicationUser> userManager)
    {
        _gradingService = gradingService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int assignmentId)
    {
        if (assignmentId <= 0)
        {
            return BadRequest();
        }

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var model =
            await _gradingService.GetAssignmentSubmissionsAsync(
                assignmentId,
                teacherId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var model = await _gradingService.GetGradeFormAsync(
            id.Value,
            teacherId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        GradeEditViewModel model)
    {
        if (id != model.SubmissionId)
        {
            return BadRequest();
        }

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var found = await PopulateDisplayFieldsAsync(
                model,
                teacherId);

            return found ? View(model) : NotFound();
        }

        try
        {
            var saved = await _gradingService.SaveGradeAsync(
                model,
                teacherId);

            if (!saved)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Grade saved successfully.";

            return RedirectToAction(nameof(Index), new
            {
                assignmentId = model.AssignmentId
            });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                nameof(model.Score),
                exception.Message);

            var found = await PopulateDisplayFieldsAsync(
                model,
                teacherId);

            return found ? View(model) : NotFound();
        }
    }

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }

    private async Task<bool> PopulateDisplayFieldsAsync(
        GradeEditViewModel model,
        string teacherId)
    {
        var storedModel =
            await _gradingService.GetGradeFormAsync(
                model.SubmissionId,
                teacherId);

        if (storedModel is null)
        {
            return false;
        }

        model.GradeId = storedModel.GradeId;
        model.AssignmentId = storedModel.AssignmentId;
        model.AssignmentTitle = storedModel.AssignmentTitle;
        model.StudentName = storedModel.StudentName;
        model.MaxPoints = storedModel.MaxPoints;

        return true;
    }
}