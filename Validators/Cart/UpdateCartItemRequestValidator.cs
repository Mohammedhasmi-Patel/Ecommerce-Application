using Ecommerce.DTO.Cart;
using FluentValidation;

namespace Ecommerce.Validators.Cart;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(c => c.ProductId)
            .Must(productId => productId != Guid.Empty)
            .WithMessage("ProductId must be a valid GUID.");

        RuleFor(c => c.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}
