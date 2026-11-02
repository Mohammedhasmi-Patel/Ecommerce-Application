namespace Ecommerce.Models;
public class EmailVerificationOtp
{
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; } = 0;
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
}

