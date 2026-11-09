using Ecommerce.Enum;

namespace Ecommerce.DTO.Order
{
    public class GetOrdersAdminRequest
    {
        public string? Search { get; set; }
        public OrderStatusEnum? OrderStatus { get; set; }
        public PaymentStatusEnum? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

