using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Product;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ModelProduct = Ecommerce.Models.Product;

namespace Ecommerce.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ProductResponse>> CreateProductAsync(CreateProductRequest request)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId) ?? throw new BadRequestException("Invalid category selected.");

            if (request.Price <= 0)
            {
                throw new BadRequestException("Price must be greater than zero.");
            }

            if (request.DiscountPrice.HasValue && request.DiscountPrice.Value >= request.Price)
            {
                throw new BadRequestException("Discount price must be less than the regular price.");
            }

            if (request.StockQuantity < 0)
            {
                throw new BadRequestException("Stock quantity cannot be negative.");
            }

            var slug = string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Name)
                : request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _context.Products
                .AnyAsync(p => p.Slug == slug);

            if (slugExists)
            {
                throw new ConflictException("A product with this slug already exists.");
            }

            string? productLogoUrl = null;
            if (request.Logo != null)
            {
                productLogoUrl = await UploadProductImageAsync(request.Logo);
            }

            var product = request.Adapt<ModelProduct>();
            product.Id = Guid.NewGuid();
            product.Name = request.Name.Trim();
            product.Description = request.Description?.Trim();
            product.Slug = slug;
            product.Logo = productLogoUrl;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = null;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var response = product.Adapt<ProductResponse>();
            response.CategoryName = category.Name;

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Product created successfully.",
                Data = response,
                StatusCode = 201
            };
        }

        #region Helper Methods

        private static async Task<string> UploadProductImageAsync(IFormFile file)
        {
            if (file.Length > 2 * 1024 * 1024)
            {
                throw new BadRequestException("Product image cannot exceed 2 MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new BadRequestException("Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var savedFilePath = Path.Combine(uploadPath, fileName);

            await using (var stream = new FileStream(savedFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/products/{fileName}";
        }

        private static string GenerateSlug(string name)
        {
            return name.Trim().ToLowerInvariant()
                .Replace(" ", "-");
        }

        #endregion
    }
}

