using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FingerPrintSystem.Infrastructure.EFCore;

public static class IdentitySeeder {
    public static async Task SeedAsync(IServiceProvider services) {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Role
        foreach (var role in new[]
                 {
                     Roles.Administrator,
                     Roles.User
                 }) {
            if (!await roleManager.RoleExistsAsync(role)) {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Admin korisnik
        const string email = "admin@gmail.com";
        const string password = "Admin@123";

        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null) {
            admin = new ApplicationUser {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded) {
                throw new Exception(string.Join(Environment.NewLine,
                    result.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Administrator)) {
            await userManager.AddToRoleAsync(admin, Roles.Administrator);
        }
    }
}