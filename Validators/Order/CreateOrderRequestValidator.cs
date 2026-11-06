using Ecommerce.DTO.Order;
using FluentValidation;

namespace Ecommerce.Validators.Order
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.AddressId)
                .NotEmpty()
                .WithMessage("Shipping address is required.")
                .NotEqual(Guid.Empty)
                .WithMessage("Invalid address ID.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithMessage("Invalid payment method.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}

