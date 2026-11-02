using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class OrderAddress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? Landmark { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
