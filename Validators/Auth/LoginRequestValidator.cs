using FluentValidation;
using Ecommerce.DTO.User;

namespace Ecommerce.Validators.Auth
{
    public class LoginRequestValidator : AbstractValidator<AdminLoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}
