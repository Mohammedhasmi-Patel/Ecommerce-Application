using Ecommerce.Common;
using Ecommerce.DTO.Cart;

namespace Ecommerce.Contracts
{
    public interface ICartService
    {
        Task<ApiResponse<CartResponse>> AddToCartAsync(Guid userId, AddToCartRequest request);
        Task<ApiResponse<CartResponse>> RemoveFromCartAsync(Guid userId, Guid productId);
        Task<ApiResponse<CartResponse>> GetCartByUserIdAsync(Guid userId);
        Task<ApiResponse<CartResponse>> UpdateCartItemQuantityAsync(Guid userId, UpdateCartItemRequest request);
        Task<ApiResponse<bool>> ClearCartAsync(Guid userId);
    }
}

