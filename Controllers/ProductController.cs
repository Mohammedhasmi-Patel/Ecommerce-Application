using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.DTO.Product;
using Ecommerce.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("create")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAsync([FromForm] CreateProductRequest request)
        {
            var response = await _productService.CreateProductAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] UpdateProductRequest request)
        {
            var response = await _productService.UpdateProductAsync(id, request);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var response = await _productService.DeleteProductAsync(id);
            return Ok(response);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<ProductResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetProductRequest request)
        {
            var response = await _productService.GetAllProductsAsync(request);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var response = await _productService.GetProductByIdAsync(id);
            return Ok(response);
        }

        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var response = await _productService.GetProductBySlugAsync(slug);
            return Ok(response);
        }
    }
}

