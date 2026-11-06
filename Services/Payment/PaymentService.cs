using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Payment;
using Ecommerce.Enum;
using Ecommerce.Services.Payment.Gateways;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IPaymentGatewayFactory _gatewayFactory;

        public PaymentService(AppDbContext context, IPaymentGatewayFactory gatewayFactory)
        {
            _context = context;
            _gatewayFactory = gatewayFactory;
        }

        public async Task<ApiResponse<PaymentOrderResult>> CreatePaymentIntentAsync(Guid userId, CreatePaymentIntentRequest request)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            if (order.PaymentStatus == PaymentStatusEnum.Paid)
            {
                throw new BadRequestException("Order is already paid.");
            }

            if (order.OrderStatus == OrderStatusEnum.Cancelled)
            {
                throw new BadRequestException("Cannot make payment for a cancelled order.");
            }

            var gateway = _gatewayFactory.GetGateway(request.Gateway);
            var result = await gateway.CreatePaymentOrderAsync(order, order.TotalAmount, "inr");

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (payment == null)
            {
                payment = new Models.Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                    GatewayTransactionId = result.GatewayTransactionId,
                    PaymentMethod = request.Gateway == PaymentGatewayEnum.Stripe ? PaymentMethodEnum.CreditCard : PaymentMethodEnum.UPI,
                    PaymentStatus = PaymentStatusEnum.Pending,
                    Amount = order.TotalAmount,
                    Currency = result.Currency.ToUpperInvariant(),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
            }
            else
            {
                payment.GatewayTransactionId = result.GatewayTransactionId;
                payment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<PaymentOrderResult>
            {
                Success = true,
                Message = "Payment intent created successfully.",
                Data = result,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<PaymentResponse>> VerifyPaymentAsync(Guid userId, VerifyPaymentRequest request)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.Id)
                ?? throw new NotFoundException("Payment record not found for this order.");

            if (payment.PaymentStatus == PaymentStatusEnum.Paid)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment has already been verified and processed.",
                    Data = payment.Adapt<PaymentResponse>(),
                    StatusCode = 200
                };
            }

            var gateway = _gatewayFactory.GetGateway(request.Gateway);
            var verifyResult = await gateway.VerifyPaymentAsync(request);

            if (!verifyResult.IsSuccess)
            {
                throw new BadRequestException(verifyResult.ErrorMessage ?? "Payment verification failed.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update Payment
                payment.PaymentStatus = PaymentStatusEnum.Paid;
                payment.TransactionId = verifyResult.TransactionId;
                payment.GatewayResponse = verifyResult.RawResponse;
                payment.PaidAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;

                // Update Order
                order.PaymentStatus = PaymentStatusEnum.Paid;
                if (order.OrderStatus == OrderStatusEnum.Pending)
                {
                    order.OrderStatus = OrderStatusEnum.Confirmed;
                }
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment verified successfully.",
                    Data = payment.Adapt<PaymentResponse>(),
                    StatusCode = 200
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

