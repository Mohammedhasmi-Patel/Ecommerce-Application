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

        public async Task<ApiResponse<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Product not found.");

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

            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                var newSlug = request.Slug.Trim().ToLowerInvariant();
                if (newSlug != product.Slug)
                {
                    var slugExists = await _context.Products
                        .AnyAsync(p => p.Slug == newSlug && p.Id != id);

                    if (slugExists)
                    {
                        throw new ConflictException("A product with this slug already exists.");
                    }
                    product.Slug = newSlug;
                }
            }

            if (request.Logo != null)
            {
                var newLogoUrl = await UploadProductImageAsync(request.Logo);
                DeleteProductImageFile(product.Logo);
                product.Logo = newLogoUrl;
            }

            request.Adapt(product);
            product.Name = request.Name.Trim();
            product.Description = request.Description?.Trim();
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = product.Adapt<ProductResponse>();
            response.CategoryName = category.Name;

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Product updated successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Product not found.");
            var hasOrders = await _context.OrderItems
                .AnyAsync(o => o.ProductId == id);

            if (hasOrders)
            {
                throw new BadRequestException("Cannot delete product linked to existing orders.");
            }

            var cartItems = await _context.CartItems
                .Where(c => c.ProductId == id)
                .ToListAsync();

            if (cartItems.Count != 0)
            {
                _context.CartItems.RemoveRange(cartItems);
            }

            DeleteProductImageFile(product.Logo);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Product deleted successfully.",
                Data = true,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<ProductResponse>> GetProductByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Product not found.");

            var response = product.Adapt<ProductResponse>();
            response.CategoryName = product.Category?.Name ?? string.Empty;

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Product retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<ProductResponse>> GetProductBySlugAsync(string slug)
        {
            var normalizedSlug = slug.Trim().ToLowerInvariant();

            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == normalizedSlug) ?? throw new NotFoundException($"Product with slug '{slug}' was not found.");

            var response = product.Adapt<ProductResponse>();
            response.CategoryName = product.Category?.Name ?? string.Empty;

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Product retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<ProductResponse>>> GetAllProductsAsync(GetProductRequest request)
        {

            var productsQuery = _context.Products.AsQueryable();
            productsQuery = ApplyProductFilters(productsQuery, request);

            if (request.Page > 0 && request.PageSize > 0)
            {
                productsQuery = productsQuery.Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);
            }

            var products = await productsQuery.Include(p => p.Category)
                                        .AsNoTracking()
                                        .ToListAsync();

            var response = products.Select(p =>
            {
                var dto = p.Adapt<ProductResponse>();
                dto.CategoryName = p.Category?.Name ?? string.Empty;
                return dto;
            }).ToList();

            return new ApiResponse<List<ProductResponse>>
            {
                Success = true,
                Message = "Products retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        #region Helper Methods

        private static IQueryable<ModelProduct> ApplyProductFilters(IQueryable<ModelProduct> query, GetProductRequest request)
        {
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(request.Search.ToLower()) || p.Slug.ToLower().Contains(request.Search.ToLower()) || p.Description.ToLower().Contains(request.Search.ToLower()));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice.Value);
            }

            if (request.InStock.HasValue)
            {
                query = request.InStock.Value ? query.Where(p => p.StockQuantity > 0) : query.Where(p => p.StockQuantity == 0);
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy) && !string.IsNullOrWhiteSpace(request.SortOrder))
            {
                bool isAscending = request.SortOrder.Equals("ASC", StringComparison.OrdinalIgnoreCase);
                query = request.SortBy.ToLower() switch
                {
                    "name" => isAscending
                                ? query.OrderBy(p => p.Name)
                                : query.OrderByDescending(p => p.Name),

                    "price" => isAscending
                                ? query.OrderBy(p => p.Price)
                                : query.OrderByDescending(p => p.Price),

                    "createdat" => isAscending
                        ? query.OrderBy(p => p.CreatedAt)
                        : query.OrderByDescending(p => p.CreatedAt),

                    _ => query

                };
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            return query;
        }
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

        private static void DeleteProductImageFile(string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(logoPath)) return;

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", logoPath.TrimStart('/', '\\'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}

