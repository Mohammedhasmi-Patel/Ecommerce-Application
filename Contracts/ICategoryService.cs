using Ecommerce.Common;
using Ecommerce.DTO.Category;

namespace Ecommerce.Contracts
{
    public interface ICategoryService
    {
        Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
        Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
        Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(Guid id);
        Task<ApiResponse<CategoryResponse>> GetCategoryBySlugAsync(string slug);
        Task<ApiResponse<List<CategoryResponse>>> GetAllCategoriesAsync(bool onlyActive = false);
        Task<ApiResponse<List<CategoryResponse>>> GetCategoryTreeAsync();
        Task<ApiResponse<bool>> ToggleCategoryStatusAsync(Guid id);
    }
}

