using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models;

public class State
{
    [Key]
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Iso2 { get; set; }
    public string? Iso3166_2 { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Type { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; set; } = []
    public ICollection<Address> Addresses { get; set; } = []
}
