using Ecommerce.Common;
using Ecommerce.DTO.User;

namespace Ecommerce.Contracts
{
    public interface IAuthService
    {
        Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest request);
        Task<ApiResponse<bool>> VerifyEmailAsync(VerifyUserRequest request);

        Task<ApiResponse<bool>> ResendOtpAsync(ResendOtpRequest request);

        Task<ApiResponse<LoginResponse>> AdminLoginAsync(AdminLoginRequest request);
    }
}
