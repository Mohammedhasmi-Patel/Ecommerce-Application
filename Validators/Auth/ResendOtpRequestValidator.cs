using Ecommerce.DTO.User;
using FluentValidation;

namespace Ecommerce.Validators.Auth
{
    public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
    {
        public ResendOtpRequestValidator()
        {
            RuleFor(o => o.Id)
                    .NotEmpty()
                    .WithMessage("Id must be required.");
        }
    }
}
