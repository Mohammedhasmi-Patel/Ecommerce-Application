using Ecommerce.DTO.User;
using FluentValidation;

namespace Ecommerce.Validators.Auth
{
    public class VerifyUserRequestValidator : AbstractValidator<VerifyUserRequest>
    {
        public VerifyUserRequestValidator()
        {
            RuleFor(u => u.Id)
                .NotEmpty()
                .WithMessage("UserId is required.");
        }
    }
}
