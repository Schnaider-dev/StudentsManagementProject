using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManager.Components;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Services;
using StudentManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var useSqlServer = builder.Environment.IsProduction();
var connectionStringName = useSqlServer
    ? "ProductionConnection"
    : "DefaultConnection";
var connectionString =
    builder.Configuration.GetConnectionString(connectionStringName)
    ?? throw new InvalidOperationException(
        $"Connection string '{connectionStringName}' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

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

// Roles are needed before development seed users can be assigned to them.
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
