using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.DTO.User;
using Ecommerce.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [ProducesResponseType(typeof(ApiResponse<List<UserAdminResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsersAdminAsync([FromQuery] GetUsersAdminRequest request)
        {
            var response = await _userService.GetAllUsersAdminAsync(request);
            return Ok(response);
        }
    }
}

