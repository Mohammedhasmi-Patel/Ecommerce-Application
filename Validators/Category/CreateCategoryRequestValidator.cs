using Ecommerce.DTO.Category;
using FluentValidation;

namespace Ecommerce.Validators.Category
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Category name is required.")
                .MaximumLength(100)
                .WithMessage("Category name cannot exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty()
                .WithMessage("Slug is required.")
                .MaximumLength(100)
                .WithMessage("Slug cannot exceed 100 characters.");
            
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.ParentCategoryId)
                .NotEqual(Guid.Empty)
                .When(x => x.ParentCategoryId.HasValue)
                .WithMessage("Invalid parent category ID.");

            RuleFor(x => x.Logo)
                .Must(file => file == null || file.Length > 0)
                .WithMessage("Logo file cannot be empty.")
                .Must(file => file == null ||
                              file.Length <= 2 * 1024 * 1024)
                .WithMessage("Logo size cannot exceed 2 MB.")
                .Must(file => file == null ||
                              new[] { ".jpg", ".jpeg", ".png", ".webp" }
                                  .Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only JPG, JPEG, PNG, and WEBP files are allowed.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order cannot be negative.");
        }
    }
}
