using Microsoft.AspNetCore.Http;

namespace Ecommerce.DTO.Category
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; }
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public IFormFile? Logo { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}

