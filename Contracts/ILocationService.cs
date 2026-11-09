using Ecommerce.Common;
using Ecommerce.DTO.Location;

namespace Ecommerce.Contracts
{
    public interface ILocationService
    {
        Task<ApiResponse<List<CountryResponse>>> GetCountriesAsync();
        Task<ApiResponse<List<StateResponse>>> GetStatesByCountryIdAsync(Guid countryId);
        Task<ApiResponse<List<CityResponse>>> GetCitiesByStateIdAsync(Guid stateId);
    }
}

