using Ecommerce.Enum;

namespace Ecommerce.DTO.Order
{
    public class CreateOrderRequest
    {
        public Guid AddressId { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.CashOnDelivery;
        public string? Notes { get; set; }
    }
}

