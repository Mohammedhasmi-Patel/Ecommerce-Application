using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Models;

public class City
{

    [Key]
    public Guid Id { get; set; }
    public Guid StateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}