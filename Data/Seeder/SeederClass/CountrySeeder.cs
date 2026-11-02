using Ecommerce.Data.Seeder.DTO;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
namespace Ecommerce.Data.Seeder.SeederClass
{
    public class CountrySeeder
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CountrySeeder(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task SeedAsync()
        {
            if (await _context.Countries.AnyAsync()) return;
            var filePath = Path.Combine(_environment.ContentRootPath,"Data", "Files", "countries+states+cities.json");
            var json = await File.ReadAllTextAsync(filePath);
            var countriesDto = JsonSerializer.Deserialize<List<CountrySeedDto>>(json);
            if (countriesDto == null) return;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var batch in countriesDto.Chunk(10))
            {
                var countries = batch.Select(c => new Country
                {
                    Id = Guid.NewGuid(),
                    Name = c.Name,
                    Iso2 = c.Iso2,
                    Iso3 = c.Iso3,
                    PhoneCode = c.PhoneCode,
                    CurrencyCode = c.CurrencyCode,
                    CurrencySymbol = c.CurrencySymbol,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    States = (c.States ?? []).Select(s => new State
                    {
                        Id = Guid.NewGuid(),
                        Name = s.Name,
                        Iso2 = s.Iso2,
                        Iso3166_2 = s.Iso3166_2,
                        Latitude = ParseDecimal(s.Latitude),
                        Longitude = ParseDecimal(s.Longitude),
                        Type = s.Type,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Cities = (s.Cities ?? []).Select(city => new City
                        {
                            Id = Guid.NewGuid(),
                            Name = city.Name,
                            Latitude = ParseDecimal(city.Latitude),
                            Longitude = ParseDecimal(city.Longitude),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }).ToList()
                    }).ToList()
                }).ToList();
                await _context.Countries.AddRangeAsync(countries);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }
            await transaction.CommitAsync();

        }

        private static decimal? ParseDecimal(string? value)
        {
            if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result))
            {
                return result;
            }

            return null;
        }

    }
}
