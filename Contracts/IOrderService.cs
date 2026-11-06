using Ecommerce.Common;
using Ecommerce.DTO.Order;

namespace Ecommerce.Contracts
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request);
        Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid userId, Guid orderId);
        Task<ApiResponse<List<OrderResponse>>> GetMyOrdersAsync(Guid userId);
        Task<ApiResponse<OrderResponse>> CancelOrderAsync(Guid userId, Guid orderId);

    }
}

