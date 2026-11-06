namespace Ecommerce.DTO.Payment
{
    public class PaymentOrderResult
    {
        public string GatewayTransactionId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string? PublishableKey { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "inr";
    }
}

