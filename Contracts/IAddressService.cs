using Ecommerce.Common;
using Ecommerce.DTO.Address;

namespace Ecommerce.Contracts
{
    public interface IAddressService
    {
        Task<ApiResponse<AddressResponse>> CreateAddressAsync(Guid userId, CreateAddressRequest request);
        Task<ApiResponse<List<AddressResponse>>> GetAddressesByUserIdAsync(Guid userId);
        Task<ApiResponse<AddressResponse>> GetAddressByIdAsync(Guid userId, Guid id);
        Task<ApiResponse<AddressResponse>> UpdateAddressAsync(Guid userId, Guid id, UpdateAddressRequest request);
        Task<ApiResponse<bool>> DeleteAddressAsync(Guid userId, Guid id);
        Task<ApiResponse<bool>> SetDefaultAddressAsync(Guid userId, Guid id);
    }
}

