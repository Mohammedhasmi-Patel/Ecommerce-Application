using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.User;
using Ecommerce.Enum;
using Ecommerce.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Ecommerce.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public AuthService(UserManager<AspNetUsers> userManager, IEmailService emailService, AppDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _context = context;
        }
        public async Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new ConflictException("User already register with this email.");
            }

            var user = request.Adapt<AspNetUsers>();
            user.UserName = request.Email;
            user.Email = request.Email;
            user.IsActive = false;
            user.EmailConfirmed = false;
            user.Role = UserRoleEnum.User;
            user.CreatedAt = DateTime.UtcNow;

            if (request.ProfileImage != null)
            {
                var file = request.ProfileImage;
                if (file.Length > 2 * 1024 * 1024)
                    throw new BadRequestException("Profile image cannot exceed 2 MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new BadRequestException("Only JPG, JPEG, PNG and WEBP images are allowed.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine(
                     Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "profiles"
                );
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                user.ProfileImageUrl = $"/uploads/profiles/{fileName}";
            }
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new BadRequestException(result.Errors.FirstOrDefault()?.Description ?? "Something went wrong while storing user info.");
            }

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            var emailOtp = new EmailVerificationOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
            };

            _context.EmailVerificationOtps.Add(emailOtp);
            await _context.SaveChangesAsync();
            await _emailService.SendOtpEmailAsync(user.Email!, otp);

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Registration successful. Please verify your email.",
                StatusCode = 201,
                Data = user.Id
            };
        }

        public async Task<ApiResponse<bool>> ResendOtpAsync(ResendOtpRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (user.EmailConfirmed)
            {
                throw new BadRequestException("Email is already verified.");
            }

            var otp = Random.Shared.Next(100000, 1000000).ToString();

            var emailOtp = new EmailVerificationOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Otp = otp,
                IsUsed = false,
            };

            var previousOtps = await _context.EmailVerificationOtps
                                .Where(u => u.UserId == request.Id && !u.IsUsed)
                                .ToListAsync();
            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;
            }
            await _context.EmailVerificationOtps.AddAsync(emailOtp);
            await _context.SaveChangesAsync();
            await _emailService.SendOtpEmailAsync(user.Email!, otp);
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "OTP resent successfully.",
                StatusCode = 200,
                Data = true
            };

        }

        public async Task<ApiResponse<bool>> VerifyEmailAsync(VerifyUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString()) ?? throw new NotFoundException("User not found.");
            if (user.EmailConfirmed)
            {
                throw new BadRequestException("Email is already verified.");
            }

            var latestOtpRecord = await _context.EmailVerificationOtps
                                                .Where(u => u.UserId == request.Id)
                                                .OrderByDescending(x => x.CreatedAt)
                                                .FirstOrDefaultAsync() ?? throw new NotFoundException("OTP not found.");
            if (latestOtpRecord.Attempts >= 5)
            {
                throw new BadRequestException("Maximum OTP attempts reached.");
            }

            if (latestOtpRecord.IsUsed)
            {
                throw new BadRequestException("The Otp already has been used.");
            }

            if (latestOtpRecord.ExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("OTP has expired.");
            }

            latestOtpRecord.Attempts++;

            if (latestOtpRecord.Otp != request.Otp)
            {
                await _context.SaveChangesAsync();

                throw new BadRequestException("Invalid OTP.");
            }
            latestOtpRecord.IsUsed = true;

            user.EmailConfirmed = true;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Email verified successfully.",
                StatusCode = 200,
                Data = true
            };
        }
    }
}
