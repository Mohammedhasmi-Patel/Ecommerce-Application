using System.Text.Json.Serialization;

namespace Ecommerce.Data.Seeder.DTO
{
    public class CountrySeedDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("iso2")]
        public string Iso2 { get; set; } = string.Empty;

        [JsonPropertyName("iso3")]
        public string Iso3 { get; set; } = string.Empty;

        [JsonPropertyName("phonecode")]
        public string? PhoneCode { get; set; }

        [JsonPropertyName("currency")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("currency_symbol")]
        public string? CurrencySymbol { get; set; }

        [JsonPropertyName("states")]
        public List<StateSeedDto> States { get; set; } = [];

    }

    public class StateSeedDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("iso2")]
        public string? Iso2 { get; set; }

        [JsonPropertyName("iso3166_2")]
        public string? Iso3166_2 { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Type { get; set; }

        public List<CitySeedDto> Cities { get; set; } = [];
    }

    public class CitySeedDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }

}
