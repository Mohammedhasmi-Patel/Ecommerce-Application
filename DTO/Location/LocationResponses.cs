namespace Ecommerce.DTO.Location
{
    public class CountryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Iso2 { get; set; } = string.Empty;
        public string Iso3 { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
    }

    public class StateResponse
    {
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Iso2 { get; set; }
        public string? Type { get; set; }
    }

    public class CityResponse
    {
        public Guid Id { get; set; }
        public Guid StateId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

