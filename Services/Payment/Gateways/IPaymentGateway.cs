using Ecommerce.DTO.Payment;
using Ecommerce.Enum;
using ModelOrder = Ecommerce.Models.Order;

namespace Ecommerce.Services.Payment.Gateways
{
    public interface IPaymentGateway
    {
        PaymentGatewayEnum Gateway { get; }

        Task<PaymentOrderResult> CreatePaymentOrderAsync(ModelOrder order, decimal amount, string currency);
        Task<PaymentVerifyResult> VerifyPaymentAsync(VerifyPaymentRequest request);
    }
}
