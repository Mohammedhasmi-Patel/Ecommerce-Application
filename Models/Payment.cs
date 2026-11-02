using Ecommerce.Enum;

namespace Ecommerce.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? GatewayTransactionId { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string? GatewayResponse { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
