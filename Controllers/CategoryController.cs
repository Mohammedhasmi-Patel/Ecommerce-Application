using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.DTO.Category;
using Ecommerce.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<CategoryResponse>> CreateAsync([FromForm] CreateCategoryRequest request)
        {
            var response = await _categoryService.CreateCategoryAsync(request);
            return StatusCode(StatusCodes.Status201Created,response);
        }
    }
}
