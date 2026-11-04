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
        private readonly ITokenService _tokenService;

        private readonly AppDbContext _context;
        private const int OtpValidityMinutes = 10;
        private const int ResendCooldownSeconds = 60;

        public AuthService(UserManager<AspNetUsers> userManager, IEmailService emailService, ITokenService tokenService, AppDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new ConflictException("User already registered with this email.");
            }

            string? savedFilePath = null;
            string? profileImageUrl = null;

            if (request.ProfileImage != null)
            {
                var file = request.ProfileImage;
                if (file.Length > 2 * 1024 * 1024)
                    throw new BadRequestException("Profile image cannot exceed 2 MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new BadRequestException("Only JPG, JPEG, PNG, and WEBP images are allowed.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                savedFilePath = Path.Combine(uploadPath, fileName);
                await using (var stream = new FileStream(savedFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                profileImageUrl = $"/uploads/profiles/{fileName}";
            }

            var user = request.Adapt<AspNetUsers>();
            user.UserName = request.Email;
            user.Email = request.Email;
            user.ProfileImageUrl = profileImageUrl;
            user.IsActive = false;
            user.EmailConfirmed = false;
            user.CreatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                if (savedFilePath != null && File.Exists(savedFilePath))
                {
                    File.Delete(savedFilePath);
                }
                var error = result.Errors.FirstOrDefault()?.Description ?? "Something went wrong while storing user info.";
                throw new BadRequestException(error);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, nameof(UserRoleEnum.User));
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                if (savedFilePath != null && File.Exists(savedFilePath))
                {
                    File.Delete(savedFilePath);
                }
                var error = roleResult.Errors.FirstOrDefault()?.Description ?? "Failed to assign role to user.";
                throw new BadRequestException(error);
            }

            var otp = GenerateSecureOtp();
            var emailOtp = new EmailVerificationOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OtpValidityMinutes),
                Attempts = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
            };

            _context.EmailVerificationOtps.Add(emailOtp);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendOtpEmailAsync(user.Email!, otp);
            }
            catch
            {
                _context.EmailVerificationOtps.Remove(emailOtp);
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);

                if (savedFilePath != null && File.Exists(savedFilePath))
                {
                    File.Delete(savedFilePath);
                }

                throw new BadRequestException("Failed to send verification email. Please verify your email configuration and try again.");
            }

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
            var user = await _userManager.FindByIdAsync(request.Id.ToString())
                ?? throw new NotFoundException("User not found.");

            if (user.EmailConfirmed)
            {
                throw new BadRequestException("Email is already verified.");
            }

            var latestOtp = await _context.EmailVerificationOtps
                .Where(u => u.UserId == request.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestOtp != null && latestOtp.CreatedAt.AddSeconds(ResendCooldownSeconds) > DateTime.UtcNow)
            {
                var remainingSeconds = (int)Math.Ceiling((latestOtp.CreatedAt.AddSeconds(ResendCooldownSeconds) - DateTime.UtcNow).TotalSeconds);
                throw new BadRequestException($"Please wait {remainingSeconds} seconds before requesting a new OTP.");
            }

            var previousOtps = await _context.EmailVerificationOtps
                .Where(u => u.UserId == request.Id && !u.IsUsed)
                .ToListAsync();

            foreach (var previousOtp in previousOtps)
            {
                previousOtp.IsUsed = true;
            }

            var otp = GenerateSecureOtp();
            var emailOtp = new EmailVerificationOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OtpValidityMinutes),
                Otp = otp,
                IsUsed = false,
            };

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
            var user = await _userManager.FindByIdAsync(request.Id.ToString())
                ?? throw new NotFoundException("User not found.");

            if (user.EmailConfirmed)
            {
                throw new BadRequestException("Email is already verified.");
            }

            var latestOtpRecord = await _context.EmailVerificationOtps
                .Where(u => u.UserId == request.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException("No OTP request found for this user.");

            if (latestOtpRecord.Attempts >= 5)
            {
                throw new BadRequestException("Maximum OTP attempts reached. Please request a new OTP.");
            }

            if (latestOtpRecord.IsUsed)
            {
                throw new BadRequestException("This OTP has already been used.");
            }

            if (latestOtpRecord.ExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("OTP has expired. Please request a new OTP.");
            }

            latestOtpRecord.Attempts++;

            if (latestOtpRecord.Otp != request.Otp)
            {
                await _context.SaveChangesAsync();
                throw new BadRequestException("Invalid OTP.");
            }

            latestOtpRecord.IsUsed = true;

            user.EmailConfirmed = true;
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Email verified successfully.",
                StatusCode = 200,
                Data = true
            };
        }

        private static string GenerateSecureOtp()
        {
            return RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        }

        public async Task<ApiResponse<LoginResponse>> AdminLoginAsync(AdminLoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException("User not found.");

            if (!user.EmailConfirmed)
            {
                throw new BadRequestException("User is not verified yet.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            string adminRole = roles.FirstOrDefault();

            if (adminRole == null || adminRole != nameof(UserRoleEnum.Admin))
            {
                throw new BadRequestException("You don't have access to this feature.");
            }

            bool isPasswordMatch = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordMatch)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            string token = await _tokenService.GenerateToken(user);

            LoginResponse response = user.Adapt<LoginResponse>();
            response.Token = token;
            response.Role = adminRole;
            return new ApiResponse<LoginResponse>()
            {
                Success = true,
                Message = "Logged in successfully.",
                Data = response,
                StatusCode = 200
            };

        }
    }
}
