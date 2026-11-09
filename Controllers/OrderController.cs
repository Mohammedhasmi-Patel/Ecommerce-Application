using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.DTO.Order;
using Ecommerce.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckoutAsync([FromBody] CreateOrderRequest request)
        {
            var userId = GetUserId();
            var response = await _orderService.CreateOrderAsync(userId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResponse<List<OrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrdersAsync()
        {
            var userId = GetUserId();
            var response = await _orderService.GetMyOrdersAsync(userId);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderByIdAsync(Guid id)
        {
            var userId = GetUserId();
            var response = await _orderService.GetOrderByIdAsync(userId, id);
            return Ok(response);
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrderAsync(Guid id)
        {
            var userId = GetUserId();
            var response = await _orderService.CancelOrderAsync(userId, id);
            return Ok(response);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [ProducesResponseType(typeof(ApiResponse<List<OrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllOrdersAdminAsync([FromQuery] GetOrdersAdminRequest request)
        {
            var response = await _orderService.GetAllOrdersAdminAsync(request);
            return Ok(response);
        }

        [HttpGet("admin/{id:guid}")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderByIdAdminAsync(Guid id)
        {
            var response = await _orderService.GetOrderByIdAdminAsync(id);
            return Ok(response);
        }

        [HttpPatch("admin/{id:guid}/status")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatusAsync(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var response = await _orderService.UpdateOrderStatusAsync(id, request);
            return Ok(response);
        }

        #region Helper Methods

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("User is not authorized or token is invalid.");
            }

            return userId;
        }

        #endregion
    }
}

