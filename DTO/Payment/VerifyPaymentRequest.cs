using Ecommerce.Enum;

namespace Ecommerce.DTO.Payment
{
    public class VerifyPaymentRequest
    {
        public Guid OrderId { get; set; }
        public PaymentGatewayEnum Gateway { get; set; } = PaymentGatewayEnum.Stripe;
        public string? PaymentIntentId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpayOrderId { get; set; }
        public string? RazorpaySignature { get; set; }
    }
}

