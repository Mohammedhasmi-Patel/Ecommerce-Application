namespace Ecommerce.DTO.User
{
    public class VerifyUserRequest
    {
        public Guid Id { get; set; }
        public string Otp { get; set; }
    }
}
