using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class AssignmentsController : Controller
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignmentsController(
        IAssignmentService assignmentService,
        ICourseService courseService,
        UserManager<ApplicationUser> userManager)
    {
        _assignmentService = assignmentService;
        _courseService = courseService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int courseId)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var course = await _courseService.GetTeacherCourseAsync(
            courseId,
            teacherId);

        if (course is null)
        {
            return NotFound();
        }

        var assignments =
            await _assignmentService.GetCourseAssignmentsAsync(
                courseId,
                teacherId);

        var model = new AssignmentIndexViewModel
        {
            CourseId = course.CourseId,
            CourseCode = course.Code,
            CourseName = course.Name,
            Assignments = assignments
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int? id)
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

        var assignment =
            await _assignmentService.GetTeacherAssignmentAsync(
                id.Value,
                teacherId);

        if (assignment is null)
        {
            return NotFound();
        }

        return View(assignment);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int courseId)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var course = await _courseService.GetTeacherCourseAsync(
            courseId,
            teacherId);

        if (course is null || !course.IsActive)
        {
            return NotFound();
        }

        ViewData["CourseName"] = course.Name;

        return View(new AssignmentCreateViewModel
        {
            CourseId = course.CourseId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AssignmentCreateViewModel model)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await SetCourseNameAsync(model.CourseId, teacherId);
            return View(model);
        }

        try
        {
            var assignment =
                await _assignmentService.CreateAssignmentAsync(
                    model,
                    teacherId);

            TempData["SuccessMessage"] =
                "Assignment was created successfully.";

            return RedirectToAction(nameof(Details), new
            {
                id = assignment.AssignmentId
            });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await SetCourseNameAsync(model.CourseId, teacherId);
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

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var assignment =
            await _assignmentService.GetTeacherAssignmentAsync(
                id.Value,
                teacherId);

        if (assignment is null || !assignment.IsActive)
        {
            return NotFound();
        }

        ViewData["CourseName"] = assignment.Course.Name;

        var model = new AssignmentEditViewModel
        {
            AssignmentId = assignment.AssignmentId,
            CourseId = assignment.CourseId,
            Title = assignment.Title,
            Description = assignment.Description,
            DueDate = assignment.DueDate,
            MaxPoints = assignment.MaxPoints
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AssignmentEditViewModel model)
    {
        if (id != model.AssignmentId)
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
            await SetCourseNameAsync(model.CourseId, teacherId);
            return View(model);
        }

        var updated =
            await _assignmentService.UpdateAssignmentAsync(
                model,
                teacherId);

        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Assignment was updated successfully.";

        return RedirectToAction(nameof(Details), new
        {
            id = model.AssignmentId
        });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
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

        var assignment =
            await _assignmentService.GetTeacherAssignmentAsync(
                id.Value,
                teacherId);

        if (assignment is null)
        {
            return NotFound();
        }

        return View(assignment);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var assignment =
            await _assignmentService.GetTeacherAssignmentAsync(
                id,
                teacherId);

        if (assignment is null)
        {
            return NotFound();
        }

        var archived =
            await _assignmentService.ArchiveAssignmentAsync(
                id,
                teacherId);

        if (!archived)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Assignment was archived successfully.";

        return RedirectToAction(nameof(Index), new
        {
            courseId = assignment.CourseId
        });
    }

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }

    private async Task SetCourseNameAsync(
        int courseId,
        string teacherId)
    {
        var course = await _courseService.GetTeacherCourseAsync(
            courseId,
            teacherId);

        ViewData["CourseName"] = course?.Name ?? "Course";
    }
}