using Ecommerce.Common.Exceptions;
using Ecommerce.Enum;

namespace Ecommerce.Services.Payment.Gateways
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
        {
            _gateways = gateways;
        }

        public IPaymentGateway GetGateway(PaymentGatewayEnum gateway)
        {
            var selectedGateway = _gateways.FirstOrDefault(g => g.Gateway == gateway) ?? throw new BadRequestException($"Payment gateway '{gateway}' is not supported or not configured.");

            return selectedGateway;
        }
    }
}

