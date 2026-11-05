using Ecommerce.Common;
using Ecommerce.DTO.Product;

namespace Ecommerce.Contracts
{
    public interface IProductService
    {
        Task<ApiResponse<ProductResponse>> CreateProductAsync(CreateProductRequest request);
        Task<ApiResponse<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request);
        Task<ApiResponse<bool>> DeleteProductAsync(Guid id);
        Task<ApiResponse<ProductResponse>> GetProductByIdAsync(Guid id);
        Task<ApiResponse<ProductResponse>> GetProductBySlugAsync(string slug);
        Task<ApiResponse<List<ProductResponse>>> GetAllProductsAsync(GetProductRequest request);
    }
}
