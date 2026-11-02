using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Models;

public class AspNetUsers : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<EmailVerificationOtp> EmailVerificationOtps { get; set; } = [];
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
}
