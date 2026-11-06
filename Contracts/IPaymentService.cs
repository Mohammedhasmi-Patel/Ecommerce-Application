using Ecommerce.Common;
using Ecommerce.DTO.Payment;

namespace Ecommerce.Contracts
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentOrderResult>> CreatePaymentIntentAsync(Guid userId, CreatePaymentIntentRequest request);
        Task<ApiResponse<PaymentResponse>> VerifyPaymentAsync(Guid userId, VerifyPaymentRequest request);
    }
}

