using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;

namespace StudentManager.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class DashboardController : Controller
{
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        IProgressService progressService,
        UserManager<ApplicationUser> userManager)
    {
        _progressService = progressService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var studentId = _userManager.GetUserId(User);

        if (studentId is null)
        {
            return Challenge();
        }

        var model =
            await _progressService.GetStudentDashboardAsync(
                studentId);

        return View(model);
    }
}