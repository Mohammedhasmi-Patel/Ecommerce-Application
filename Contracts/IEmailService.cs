namespace Ecommerce.Contracts
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string targetEmail, string otpCode);
    }
}
