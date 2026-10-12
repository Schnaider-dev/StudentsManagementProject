using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Services;
using StudentManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IAssignmentService, AssignmentService>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<ISubmissionService, SubmissionService>();

builder.Services.AddScoped<IGradingService, GradingService>();

builder.Services.AddScoped<IProgressService, ProgressService>();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Blazor Web App using static server-side rendering with enhanced navigation and forms.
// No interactive render mode is enabled, so each request gets its own scoped DbContext.
builder.Services.AddRazorComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<FlashService>();

// The Identity UI (login, register, account management) is still served as Razor Pages.
builder.Services.AddRazorPages();

var app = builder.Build();

// Roles are needed before development seed users can be assigned to them.
await DbInitializer.SeedRolesAsync(app.Services);

// Keep demo credentials and sample school data out of non-development environments.
if (app.Environment.IsDevelopment())
{
    await SeedData.SeedAsync(app.Services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Unknown URLs render the Blazor NotFound page.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseRouting();

// Authentication must populate the user principal before role authorization runs.
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery must run after authentication/authorization and before the endpoints.
app.UseAntiforgery();

app.UseMiddleware<FlashMiddleware>();

app.MapStaticAssets();

app.MapRazorComponents<StudentManager.Components.App>();

app.MapRazorPages();

app.Run();
