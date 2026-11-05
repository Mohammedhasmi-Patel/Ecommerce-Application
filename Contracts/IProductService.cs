using Ecommerce.Common;
using Ecommerce.DTO.Product;

namespace Ecommerce.Contracts
{
    public interface IProductService
    {
        Task<ApiResponse<ProductResponse>> CreateProductAsync(CreateProductRequest request);
    }
}
