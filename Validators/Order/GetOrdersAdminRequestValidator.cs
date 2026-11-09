using Ecommerce.DTO.Order;
using FluentValidation;

namespace Ecommerce.Validators.Order
{
    public class GetOrdersAdminRequestValidator : AbstractValidator<GetOrdersAdminRequest>
    {
        public GetOrdersAdminRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.OrderStatus)
                .IsInEnum()
                .When(x => x.OrderStatus.HasValue)
                .WithMessage("Invalid order status.");

            RuleFor(x => x.PaymentStatus)
                .IsInEnum()
                .When(x => x.PaymentStatus.HasValue)
                .WithMessage("Invalid payment status.");

            RuleFor(x => x)
                .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
                .WithMessage("FromDate cannot be later than ToDate.");
        }
    }
}

