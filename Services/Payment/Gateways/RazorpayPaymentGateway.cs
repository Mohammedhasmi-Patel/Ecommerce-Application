using Ecommerce.DTO.Payment;
using Ecommerce.Enum;
using ModelOrder = Ecommerce.Models.Order;

namespace Ecommerce.Services.Payment.Gateways
{
    public class RazorpayPaymentGateway : IPaymentGateway
    {
        public PaymentGatewayEnum Gateway => PaymentGatewayEnum.Razorpay;

        public Task<PaymentOrderResult> CreatePaymentOrderAsync(ModelOrder order, decimal amount, string currency)
        {
            throw new NotImplementedException("Razorpay gateway is not implemented yet.");
        }

        public Task<PaymentVerifyResult> VerifyPaymentAsync(VerifyPaymentRequest request)
        {
            throw new NotImplementedException("Razorpay gateway is not implemented yet.");
        }
    }
}
