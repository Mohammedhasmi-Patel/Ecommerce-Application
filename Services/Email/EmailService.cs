using Ecommerce.Configurations;
using Ecommerce.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Ecommerce.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendOtpEmailAsync(string targetEmail, string otpCode)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(targetEmail));
            email.Subject = "Your Secure One-Time Password (OTP)";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 30px; max-width: 500px; margin: auto; border: 1px solid #eee; border-radius: 10px;'>
                        <h2 style='color: #2c3e50;'>Email Verification</h2>
                        <p>Please use the verification code below to complete your registration.</p>

                        <div style='background: #f4f4f4; padding: 20px; text-align: center; border-radius: 8px;'>
                            <h1 style='color: #2c3e50; letter-spacing: 8px; font-size: 32px; margin: 0;'>{otpCode}</h1>
                        </div>

                        <p style='margin-top: 20px;'>
                            This OTP is valid for <strong>10 minutes</strong>. Please do not share it with anyone.
                        </p>

                        <p style='color: #7f8c8d; font-size: 12px;'>
                            If you did not request this verification code, please ignore this email.
                        </p>

                        <hr style='border: none; border-top: 1px solid #eee; margin-top: 25px;' />

                        <p style='color: #999; font-size: 12px; text-align: center;'>
                            This is an automated email. Please do not reply.
                        </p>
                    </div>"
            };
            email.Body = bodyBuilder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Server, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.SenderEmail,_mailSettings.Secret);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

        }
    }
}
