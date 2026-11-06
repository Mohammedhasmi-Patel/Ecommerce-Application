using Ecommerce.Enum;

namespace Ecommerce.DTO.Payment
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? GatewayTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string PaymentMethodName => PaymentMethod.ToString();
        public PaymentStatusEnum PaymentStatus { get; set; }
        public string PaymentStatusName => PaymentStatus.ToString();
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

