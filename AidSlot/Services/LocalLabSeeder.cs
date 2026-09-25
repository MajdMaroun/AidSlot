using AidSlot.Data;
using AidSlot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AidSlot.Services;

public static class LocalLabSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var suffix in new[] { "A", "B" })
        {
            var email = Environment.GetEnvironmentVariable($"AIDSLOT_LAB_{suffix}_EMAIL");
            var password = Environment.GetEnvironmentVariable($"AIDSLOT_LAB_{suffix}_PASSWORD");
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"Set the lab {suffix} email and password environment variables.");

            var name = $"Synthetic Organization {suffix}";
            var organization = await db.Organizations.SingleOrDefaultAsync(o => o.Name == name);
            if (organization is null)
            {
                organization = new Organization { Name = name };
                db.Organizations.Add(organization);
                await db.SaveChangesAsync();
            }

            var user = await users.FindByEmailAsync(email);
            if (user is not null)
            {
                if (user.OrganizationId != organization.Id)
                    throw new InvalidOperationException($"Lab {suffix} email already belongs to a different account or organization.");
                continue;
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = $"Synthetic Admin {suffix}",
                OrganizationId = organization.Id
            };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
                throw new InvalidOperationException($"Could not create lab {suffix} user: {string.Join(", ", created.Errors.Select(e => e.Code))}");
            var assigned = await users.AddToRoleAsync(user, "Admin");
            if (!assigned.Succeeded)
                throw new InvalidOperationException($"Could not assign Admin role to lab {suffix} user.");
        }
    }
}
