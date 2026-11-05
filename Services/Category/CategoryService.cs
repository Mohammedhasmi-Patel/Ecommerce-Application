using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Category;
using Microsoft.EntityFrameworkCore;
using ModelCategory = Ecommerce.Models.Category;

namespace Ecommerce.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var slug = string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Name)
                : request.Slug.Trim().ToLowerInvariant();

            var slugExists = await _context.Categories
                .AnyAsync(x => x.Slug == slug);

            if (slugExists)
            {
                throw new ConflictException("A category with this slug already exists.");
            }

            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ParentCategoryId.Value) ?? throw new BadRequestException("Invalid parent category.");
            }

            string? categoryLogoUrl = null;
            if (request.Logo != null)
            {
                categoryLogoUrl = await UploadLogoAsync(request.Logo);
            }

            var createCategory = new ModelCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                Logo = categoryLogoUrl,
                ParentCategoryId = request.ParentCategoryId,
                Slug = slug,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _context.Categories.AddAsync(createCategory);
            await _context.SaveChangesAsync();

            var response = MapToResponse(createCategory);

            return new ApiResponse<CategoryResponse>
            {
                Success = true,
                Message = "Category created successfully.",
                Data = response,
                StatusCode = 201
            };
        }

        public async Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id) ?? throw new NotFoundException("Category not found.");

            if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == id)
                {
                    throw new BadRequestException("A category cannot be its own parent.");
                }

                var parent = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.ParentCategoryId.Value) ?? throw new BadRequestException("Invalid parent category.");
            }

            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                var newSlug = request.Slug.Trim().ToLowerInvariant();
                if (newSlug != category.Slug)
                {
                    var slugExists = await _context.Categories
                        .AnyAsync(c => c.Slug == newSlug && c.Id != id);

                    if (slugExists)
                    {
                        throw new ConflictException("A category with this slug already exists.");
                    }

                    category.Slug = newSlug;
                }
            }

            if (request.Logo != null)
            {
                var newLogoUrl = await UploadLogoAsync(request.Logo);
                DeleteLogoFile(category.Logo);
                category.Logo = newLogoUrl;
            }

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();
            category.ParentCategoryId = request.ParentCategoryId;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = MapToResponse(category);

            return new ApiResponse<CategoryResponse>
            {
                Success = true,
                Message = "Category updated successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id) ?? throw new NotFoundException("Category not found.");
            var hasSubCategories = await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == id);

            if (hasSubCategories)
            {
                throw new BadRequestException("Cannot delete this category because it contains sub-categories. Please delete or reassign them first.");
            }

            var hasProducts = await _context.Products
                .AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                throw new BadRequestException("Cannot delete this category because it has associated products. Please reassign or delete the products first.");
            }

            DeleteLogoFile(category.Logo);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Category deleted successfully.",
                Data = true,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id) ?? throw new NotFoundException("Category not found.");

            var response = MapToResponseWithSubCategories(category);

            return new ApiResponse<CategoryResponse>
            {
                Success = true,
                Message = "Category retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<CategoryResponse>> GetCategoryBySlugAsync(string slug)
        {
            var normalizedSlug = slug.Trim().ToLowerInvariant();

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Slug == normalizedSlug) ?? throw new NotFoundException($"Category with slug '{slug}' was not found.");

            var response = MapToResponseWithSubCategories(category);

            return new ApiResponse<CategoryResponse>
            {
                Success = true,
                Message = "Category retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<CategoryResponse>>> GetAllCategoriesAsync()
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .AsNoTracking();

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var response = categories.Select(c => MapToResponse(c)).ToList();

            return new ApiResponse<List<CategoryResponse>>
            {
                Success = true,
                Message = "Categories retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<bool>> ToggleCategoryStatusAsync(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id) ?? throw new NotFoundException("Category not found.");

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = $"Category status changed to {(category.IsActive ? "active" : "inactive")}.",
                Data = category.IsActive,
                StatusCode = 200
            };
        }

        #region Helper Methods
        private static async Task<string> UploadLogoAsync(IFormFile file)
        {
            if (file.Length > 2 * 1024 * 1024)
            {
                throw new BadRequestException("Category image cannot exceed 2 MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new BadRequestException("Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
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

            return $"/uploads/categories/{fileName}";
        }

        private static void DeleteLogoFile(string? logoPath)
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
                // Suppress file delete failure to avoid aborting database operations
            }
        }

        private static string GenerateSlug(string name)
        {
            return name.Trim().ToLowerInvariant()
                .Replace(" ", "-");
        }

        private static CategoryResponse MapToResponse(ModelCategory c)
        {
            return new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                Logo = c.Logo,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                SubCategories = []
            };
        }

        private static CategoryResponse MapToResponseWithSubCategories(ModelCategory c)
        {
            var response = MapToResponse(c);
            response.SubCategories = c.SubCategories
                .Select(s => MapToResponse(s))
                .ToList();
            return response;
        }

        #endregion
    }
}
