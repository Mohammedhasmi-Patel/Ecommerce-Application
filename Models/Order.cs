using Ecommerce.Enum;

namespace Ecommerce.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal ShippingAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;
    public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public AspNetUsers User { get; set; } = null!;
    public OrderAddress? OrderAddress { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
