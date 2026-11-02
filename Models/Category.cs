using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;
public class Category
{
    [Key]
    public Guid Id { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

