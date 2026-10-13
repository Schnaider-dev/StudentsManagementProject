using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManager.Components;
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

builder.Services.AddRazorPages();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Create the database and apply pending migrations during application startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

// Roles are needed before users can be assigned to them.
await DbInitializer.SeedRolesAsync(app.Services);

// Keep demo credentials and sample school data out of non-development environments.
if (app.Environment.IsDevelopment())
{
    await SeedData.SeedAsync(app.Services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Authentication must populate the user principal before role authorization runs.
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorPages();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
