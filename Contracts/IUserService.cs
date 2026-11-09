using Ecommerce.Common;
using Ecommerce.DTO.User;

namespace Ecommerce.Contracts
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserAdminResponse>>> GetAllUsersAdminAsync(GetUsersAdminRequest request);
    }
}

