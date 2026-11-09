using Ecommerce.Enum;

namespace Ecommerce.DTO.Order
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatusEnum OrderStatus { get; set; }
    }
}

