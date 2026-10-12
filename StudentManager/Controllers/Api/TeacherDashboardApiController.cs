using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Controllers.Api;

[ApiController]
[Route("api/teacher/dashboard")]
[Authorize(Roles = "Teacher")]
public class TeacherDashboardApiController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherDashboardApiController(
        IProgressService progressService,
        UserManager<ApplicationUser> userManager)
    {
        _progressService = progressService;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType<TeacherDashboardViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeacherDashboardViewModel>> GetDashboard()
    {
        var teacherId = _userManager.GetUserId(User);

        if (teacherId is null)
        {
            return Unauthorized();
        }

        return await _progressService.GetTeacherDashboardAsync(teacherId);
    }
}
