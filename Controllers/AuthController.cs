using Ecommerce.Contracts;
using Ecommerce.DTO.User;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Common;

namespace Ecommerce.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]

    public async Task<IActionResult> CreateUserAsync([FromForm] CreateUserRequest request)
    {
        var response = await _authService.CreateUserAsync(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("verify-otp")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> VerifyUserAsync([FromBody] VerifyUserRequest request)
    {
        var response = await _authService.VerifyEmailAsync(request);
        return Ok(response);
    }

    [HttpPost("resend-otp")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> ResendOtpAsync([FromBody] ResendOtpRequest request)
    {
        var response = await _authService.ResendOtpAsync(request);
        return Ok(response);
    }
}
