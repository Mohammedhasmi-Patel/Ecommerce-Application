using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.DTO.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Route("api/locations")]
    [ApiController]
    [AllowAnonymous]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("countries")]
        [ProducesResponseType(typeof(ApiResponse<List<CountryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCountriesAsync()
        {
            var response = await _locationService.GetCountriesAsync();
            return Ok(response);
        }

        [HttpGet("countries/{countryId:guid}/states")]
        [ProducesResponseType(typeof(ApiResponse<List<StateResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatesByCountryIdAsync(Guid countryId)
        {
            var response = await _locationService.GetStatesByCountryIdAsync(countryId);
            return Ok(response);
        }

        [HttpGet("states/{stateId:guid}/cities")]
        [ProducesResponseType(typeof(ApiResponse<List<CityResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCitiesByStateIdAsync(Guid stateId)
        {
            var response = await _locationService.GetCitiesByStateIdAsync(stateId);
            return Ok(response);
        }
    }
}

