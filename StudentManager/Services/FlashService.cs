namespace StudentManager.Services;

/// <summary>The kind of flash message to show.</summary>
public enum FlashKind
{
    Success,
    Error
}

/// <summary>
/// One-shot messages that survive a redirect (the Blazor replacement for TempData).
/// The message is stored in a short-lived cookie by <see cref="Set"/> and picked up by
/// <see cref="FlashMiddleware"/> on the next request.
/// </summary>
public class FlashService
{
    public const string CookieName = "sm_flash";
    public const string ItemKey = "sm_flash";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public FlashService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Success(string message) => Set(FlashKind.Success, message);

    public void Error(string message) => Set(FlashKind.Error, message);

    /// <summary>The message that arrived with this request, if any.</summary>
    public (FlashKind Kind, string Message)? Current
        => _httpContextAccessor.HttpContext?.Items[ItemKey] as (FlashKind, string)?;

    private void Set(FlashKind kind, string message)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context is null || context.Response.HasStarted)
        {
            return;
        }

        context.Response.Cookies.Append(
            CookieName,
            Uri.EscapeDataString($"{kind}|{message}"),
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                MaxAge = TimeSpan.FromMinutes(1),
                IsEssential = true
            });
    }
}

/// <summary>Moves a pending flash cookie into HttpContext.Items and clears it.</summary>
public class FlashMiddleware
{
    private readonly RequestDelegate _next;

    public FlashMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(FlashService.CookieName, out var raw))
        {
            var parts = Uri.UnescapeDataString(raw).Split('|', 2);

            if (parts.Length == 2 && Enum.TryParse<FlashKind>(parts[0], out var kind))
            {
                context.Items[FlashService.ItemKey] = (kind, parts[1]);
            }

            context.Response.Cookies.Delete(FlashService.CookieName);
        }

        await _next(context);
    }
}
