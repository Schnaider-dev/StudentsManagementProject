using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using StudentManager.Models;

namespace StudentManager.Services;

/// <summary>
/// Resolves the signed-in user for Blazor pages, replacing the MVC controllers' User property.
/// </summary>
public class CurrentUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserService(
        AuthenticationStateProvider authenticationStateProvider,
        UserManager<ApplicationUser> userManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;
    }

    /// <summary>Returns the Identity user id, or null when nobody is signed in.</summary>
    public async Task<string?> GetUserIdAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();

        return state.User.Identity?.IsAuthenticated == true
            ? _userManager.GetUserId(state.User)
            : null;
    }

    public async Task<ClaimsPrincipal> GetPrincipalAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();

        return state.User;
    }
}
