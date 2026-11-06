namespace Ecommerce.DTO.Payment
{
    public class PaymentVerifyResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RawResponse { get; set; }
    }
}

