using Ecommerce.Enum;

namespace Ecommerce.DTO.Address
{
    public class CreateAddressRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public Guid CountryId { get; set; }
        public Guid StateId { get; set; }
        public Guid CityId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string? Landmark { get; set; }
        public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Home;
        public bool IsDefault { get; set; } = false;
    }
}
