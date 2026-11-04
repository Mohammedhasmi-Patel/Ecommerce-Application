using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Category;
using Ecommerce.Models;
using Mapster;
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
            //throw new NotImplementedException();

            var slugExists = await _context.Categories
                .AnyAsync(x => x.Slug == request.Slug);

            if (slugExists)
            {
                throw new BadRequestException("The slug already exists.");
            }

            if (request.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(x => x.Id == request.ParentCategoryId.Value);

                if (!parentExists)
                {
                    throw new BadRequestException("Invalid parent category.");
                }
            }

            string? categoryUrl = null;
            if (request.Logo != null)
            {
                var file = request.Logo;

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

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","uploads","categories");
                var fileName = $"{Guid.NewGuid()}{extension}";
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string savedFilePath = Path.Combine(uploadPath, fileName);
                await using (var stream = new FileStream(savedFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                categoryUrl = $"/uploads/categories/{fileName}";
            }

            var createCategory = new ModelCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                Logo = categoryUrl,
                ParentCategoryId = request.ParentCategoryId,
                Slug = request.Slug,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _context.Categories.AddAsync(createCategory);
            await _context.SaveChangesAsync();

            var response = createCategory.Adapt<CategoryResponse>();
            return new ApiResponse<CategoryResponse>
            {
                Success = true,
                Message = "Category created successfully.",
                Data = response,
                StatusCode = 201
            };

        }

        public Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<CategoryResponse>>> GetAllCategoriesAsync(bool onlyActive = false)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<CategoryResponse>> GetCategoryBySlugAsync(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<CategoryResponse>>> GetCategoryTreeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ToggleCategoryStatusAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            throw new NotImplementedException();
        }

    }
}
