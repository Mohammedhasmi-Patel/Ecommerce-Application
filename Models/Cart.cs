namespace Ecommerce.Models;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public AspNetUsers User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];
}
