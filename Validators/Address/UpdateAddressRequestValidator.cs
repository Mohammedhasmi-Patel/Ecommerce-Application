using Ecommerce.DTO.Address;
using FluentValidation;

namespace Ecommerce.Validators.Address
{
    public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
    {
        public UpdateAddressRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Full name is required.")
                .MaximumLength(150)
                .WithMessage("Full name cannot exceed 150 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters.");

            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .WithMessage("Address line 1 is required.")
                .MaximumLength(250)
                .WithMessage("Address line 1 cannot exceed 250 characters.");

            RuleFor(x => x.AddressLine2)
                .MaximumLength(250)
                .WithMessage("Address line 2 cannot exceed 250 characters.");

            RuleFor(x => x.CountryId)
                .NotEmpty()
                .WithMessage("Country is required.")
                .NotEqual(Guid.Empty)
                .WithMessage("Invalid country ID.");

            RuleFor(x => x.StateId)
                .NotEmpty()
                .WithMessage("State is required.")
                .NotEqual(Guid.Empty)
                .WithMessage("Invalid state ID.");

            RuleFor(x => x.CityId)
                .NotEmpty()
                .WithMessage("City is required.")
                .NotEqual(Guid.Empty)
                .WithMessage("Invalid city ID.");

            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required.")
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters.");

            RuleFor(x => x.Landmark)
                .MaximumLength(200)
                .WithMessage("Landmark cannot exceed 200 characters.");

            RuleFor(x => x.AddressType)
                .IsInEnum()
                .WithMessage("Invalid address type.");
        }
    }
}

