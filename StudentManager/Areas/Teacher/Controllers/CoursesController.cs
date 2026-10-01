using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesController(
        ICourseService courseService,
        UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        var courses = await _courseService.GetTeacherCoursesAsync(teacherId);

        return View(courses);
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

        var course = await _courseService.GetTeacherCourseAsync(
            id.Value,
            teacherId);

        if (course is null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CourseCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        try
        {
            var course = await _courseService.CreateCourseAsync(
                model,
                teacherId);

            TempData["SuccessMessage"] =
                $"Course {course.Code} was created successfully.";

            return RedirectToAction(nameof(Details), new
            {
                id = course.CourseId
            });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                nameof(model.Code),
                exception.Message);

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

        var course = await _courseService.GetTeacherCourseAsync(
            id.Value,
            teacherId);

        if (course is null)
        {
            return NotFound();
        }

        var model = new CourseEditViewModel
        {
            CourseId = course.CourseId,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CourseEditViewModel model)
    {
        if (id != model.CourseId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var teacherId = GetCurrentUserId();

        if (teacherId is null)
        {
            return Challenge();
        }

        try
        {
            var updated = await _courseService.UpdateCourseAsync(
                model,
                teacherId);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Course was updated successfully.";

            return RedirectToAction(nameof(Details), new
            {
                id = model.CourseId
            });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(
                nameof(model.Code),
                exception.Message);

            return View(model);
        }
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

        var course = await _courseService.GetTeacherCourseAsync(
            id.Value,
            teacherId);

        if (course is null)
        {
            return NotFound();
        }

        return View(course);
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

        var archived = await _courseService.ArchiveCourseAsync(
            id,
            teacherId);

        if (!archived)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Course was archived successfully.";

        return RedirectToAction(nameof(Index));
    }

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }
}