namespace Ecommerce.DTO.Cart
{
    public class UpdateCartItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

