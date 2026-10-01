using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;

namespace StudentManager.Areas.Teacher.Controllers;

[Area("Teacher")]
[Authorize(Roles = "Teacher")]
public class StudentsController : Controller
{
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentsController(
        IProgressService progressService,
        UserManager<ApplicationUser> userManager)
    {
        _progressService = progressService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int courseId)
    {
        if (courseId <= 0)
        {
            return BadRequest();
        }

        var teacherId = _userManager.GetUserId(User);

        if (teacherId is null)
        {
            return Challenge();
        }

        var model = await _progressService.GetCourseStudentsAsync(
            courseId,
            teacherId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }
}