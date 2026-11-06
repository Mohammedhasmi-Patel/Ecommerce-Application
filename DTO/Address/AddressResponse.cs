using Ecommerce.Enum;

namespace Ecommerce.DTO.Address
{
    public class AddressResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public Guid CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public Guid StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? Landmark { get; set; }
        public AddressTypeEnum AddressType { get; set; }
        public string AddressTypeName => AddressType.ToString();
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

