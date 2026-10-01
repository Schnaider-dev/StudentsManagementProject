using Microsoft.AspNetCore.Identity;

namespace StudentManager.Data;

public static class DbInitializer
{
    private static readonly string[] Roles =
    {
        "Teacher",
        "Student"
    };

    public static async Task SeedRolesAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create role '{roleName}': {errors}");
            }
        }
    }
}