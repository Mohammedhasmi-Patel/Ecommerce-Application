using Ecommerce.Configurations;
using Ecommerce.Enum;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Ecommerce.Data.Seeder.SeederClass
{
    public class SuperAdminSeeder
    {
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly SuperAdminSettings _settings;
        private readonly ILogger<SuperAdminSeeder> _logger;

        public SuperAdminSeeder(
            UserManager<AspNetUsers> userManager,
            IOptions<SuperAdminSettings> options,
            ILogger<SuperAdminSeeder> logger)
        {
            _userManager = userManager;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var existingUser = await _userManager.FindByEmailAsync(_settings.Email);

            if (existingUser == null)
            {
                var adminUser = new AspNetUsers
                {
                    Id = Guid.NewGuid(),
                    UserName = _settings.Email,
                    Email = _settings.Email,
                    FirstName = _settings.FirstName,
                    LastName = _settings.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(adminUser, _settings.Password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to seed admin user: {Errors}", errors);
                    throw new InvalidOperationException($"Failed to seed admin user: {errors}");
                }

                await _userManager.AddToRoleAsync(adminUser, nameof(UserRoleEnum.Admin));

                _logger.LogInformation("Admin user '{Email}' created and assigned Admin role successfully.", _settings.Email);
            }
        }
    }
}
