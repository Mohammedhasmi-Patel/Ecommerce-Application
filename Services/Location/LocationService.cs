using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Location;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Location
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<CountryResponse>>> GetCountriesAsync()
        {
            var countries = await _context.Countries
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new ApiResponse<List<CountryResponse>>
            {
                Success = true,
                Message = "Countries fetched successfully.",
                Data = countries.Adapt<List<CountryResponse>>(),
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<StateResponse>>> GetStatesByCountryIdAsync(Guid countryId)
        {
            var states = await _context.States
                .AsNoTracking()
                .Where(s => s.CountryId == countryId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new ApiResponse<List<StateResponse>>
            {
                Success = true,
                Message = "States fetched successfully.",
                Data = states.Adapt<List<StateResponse>>(),
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<CityResponse>>> GetCitiesByStateIdAsync(Guid stateId)
        {
            var cities = await _context.Cities
                .AsNoTracking()
                .Where(c => c.StateId == stateId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new ApiResponse<List<CityResponse>>
            {
                Success = true,
                Message = "Cities fetched successfully.",
                Data = cities.Adapt<List<CityResponse>>(),
                StatusCode = 200
            };
        }
    }
}

