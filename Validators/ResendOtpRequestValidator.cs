using Ecommerce.DTO.User;
using FluentValidation;

namespace Ecommerce.Validators
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
