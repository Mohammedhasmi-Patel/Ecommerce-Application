using Ecommerce.DTO.Order;
using FluentValidation;

namespace Ecommerce.Validators.Order
{
    public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator()
        {
            RuleFor(x => x.OrderStatus)
                .IsInEnum()
                .WithMessage("Invalid order status.");
        }
    }
}

