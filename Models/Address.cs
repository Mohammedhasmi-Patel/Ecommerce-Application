using System.ComponentModel.DataAnnotations;
using Ecommerce.Enum;

namespace Ecommerce.Models;

public class Address
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public Guid CountryId { get; set; }
    public Guid StateId { get; set; }
    public Guid CityId { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string? Landmark { get; set; }
    public AddressTypeEnum AddressType { get; set; }
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public AspNetUsers User { get; set; } = null!;
    public Country Country { get; set; } = null!;
    public State State { get; set; } = null!;
    public City City { get; set; } = null!;
}
