using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.DTO.Category;
using Ecommerce.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("create")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAsync([FromForm] CreateCategoryRequest request)
        {
            var response = await _categoryService.CreateCategoryAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] UpdateCategoryRequest request)
        {
            var response = await _categoryService.UpdateCategoryAsync(id, request);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var response = await _categoryService.DeleteCategoryAsync(id);
            return Ok(response);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _categoryService.GetAllCategoriesAsync();
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var response = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(response);
        }

        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var response = await _categoryService.GetCategoryBySlugAsync(slug);
            return Ok(response);
        }

        [HttpPatch("{id:guid}/toggle-status")]
        [Authorize(Roles = nameof(UserRoleEnum.Admin))]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatusAsync(Guid id)
        {
            var response = await _categoryService.ToggleCategoryStatusAsync(id);
            return Ok(response);
        }
    }
}
