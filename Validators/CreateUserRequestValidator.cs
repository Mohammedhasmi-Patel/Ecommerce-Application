using Ecommerce.DTO.User;
using FluentValidation;

namespace Ecommerce.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100);

            RuleFor(x => x.ProfileImage)
                .Must(file => file == null || file.Length <= 2 * 1024 * 1024)
                .WithMessage("Profile image size must not exceed 2 MB.")
                .Must(file => file == null ||
                    new[] { ".jpg", ".jpeg", ".png", ".webp" }
                        .Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

    }
}
