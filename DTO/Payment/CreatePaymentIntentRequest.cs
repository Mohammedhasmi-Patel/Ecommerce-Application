using Ecommerce.Enum;

namespace Ecommerce.DTO.Payment
{
    public class CreatePaymentIntentRequest
    {
        public Guid OrderId { get; set; }
        public PaymentGatewayEnum Gateway { get; set; } = PaymentGatewayEnum.Stripe;
    }
}

