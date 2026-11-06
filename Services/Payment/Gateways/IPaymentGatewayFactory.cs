using Ecommerce.Enum;

namespace Ecommerce.Services.Payment.Gateways
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway GetGateway(PaymentGatewayEnum gateway);
    }
}

