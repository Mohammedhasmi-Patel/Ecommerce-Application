namespace Ecommerce.DTO.Cart
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemResponse> Items { get; set; } = [];
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
        public decimal TotalSavings => SubTotal - TotalAmount;
        public bool HasDiscount => TotalSavings > 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductLogo { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal EffectivePrice => DiscountPrice ?? UnitPrice;
        public int Quantity { get; set; }
        public decimal TotalPrice => EffectivePrice * Quantity;
    }
}

