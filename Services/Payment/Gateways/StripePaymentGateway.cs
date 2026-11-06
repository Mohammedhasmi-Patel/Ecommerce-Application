using Ecommerce.Configurations;
using Ecommerce.DTO.Payment;
using Ecommerce.Enum;
using Microsoft.Extensions.Options;
using Stripe;
using ModelOrder = Ecommerce.Models.Order;

namespace Ecommerce.Services.Payment.Gateways
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly StripeSettings _stripeSettings;

        public StripePaymentGateway(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.STRIPE_SECRET_KEY;
        }

        public PaymentGatewayEnum Gateway => PaymentGatewayEnum.Stripe;

        public async Task<PaymentOrderResult> CreatePaymentOrderAsync(ModelOrder order, decimal amount, string currency)
        {
            var amountInSmallestUnit = (long)(amount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInSmallestUnit,
                Currency = string.IsNullOrWhiteSpace(currency) ? "inr" : currency.ToLowerInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Description = $"Payment for Order #{order.OrderNumber}",
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.Id.ToString() },
                    { "OrderNumber", order.OrderNumber },
                    { "UserId", order.UserId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.CreateAsync(options);

            return new PaymentOrderResult
            {
                GatewayTransactionId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                PublishableKey = _stripeSettings.STRIPE_API_KEY,
                Amount = amount,
                Currency = currency
            };
        }

        public async Task<PaymentVerifyResult> VerifyPaymentAsync(VerifyPaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentIntentId))
            {
                return new PaymentVerifyResult
                {
                    IsSuccess = false,
                    ErrorMessage = "PaymentIntentId is required for Stripe verification."
                };
            }

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.GetAsync(request.PaymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                return new PaymentVerifyResult
                {
                    IsSuccess = true,
                    TransactionId = paymentIntent.LatestChargeId ?? paymentIntent.Id,
                    RawResponse = paymentIntent.ToJson()
                };
            }

            return new PaymentVerifyResult
            {
                IsSuccess = false,
                ErrorMessage = $"Payment not completed. Stripe status: '{paymentIntent.Status}'."
            };
        }
    }
}
