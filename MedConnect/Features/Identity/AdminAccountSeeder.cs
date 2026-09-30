using MedConnect.Common.Constants;
using MedConnect.Domain;
using Microsoft.AspNetCore.Identity;

namespace MedConnect.Features.Identity;

/// Creates the very first Admin account so staff can be provisioned at all
/// (self-registration is patient-only, and only an Admin can create staff).
/// Runs only when configuration supplies AdminBootstrap:Email/Password and no
/// Admin user exists yet — so it is a no-op in production unless configured.
public static class AdminAccountSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var email = config["AdminBootstrap:Email"];
        var password = config["AdminBootstrap:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var existingAdmins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        if (existingAdmins.Count > 0)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "System Administrator"
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}