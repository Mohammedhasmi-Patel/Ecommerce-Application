using Ecommerce.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data.Seeder.SeederClass;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private static readonly string[] Roles = [
        nameof(UserRoleEnum.Admin),
        nameof(UserRoleEnum.User)
    ];

    public RoleSeeder(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        var existingRoles = await _roleManager.Roles
            .Select(r => r.Name)
            .ToListAsync();

        var missingRoles = Roles
            .Except(existingRoles)
            .ToArray();

        foreach (var role in missingRoles)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role '{role}'.");
            }
        }
    }
}
