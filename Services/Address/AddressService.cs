using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Address;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ModelAddress = Ecommerce.Models.Address;

namespace Ecommerce.Services.Address
{
    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;

        public AddressService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<AddressResponse>> CreateAddressAsync(Guid userId, CreateAddressRequest request)
        {
            var country = await _context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CountryId && c.IsActive)
                ?? throw new NotFoundException("Selected country not found or is inactive.");

            var state = await _context.States
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.StateId && s.CountryId == request.CountryId && s.IsActive)
                ?? throw new BadRequestException("Selected state does not belong to the selected country or is inactive.");

            var city = await _context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CityId && c.StateId == request.StateId && c.IsActive)
                ?? throw new BadRequestException("Selected city does not belong to the selected state or is inactive.");

            var hasExistingAddresses = await _context.Addresses
                .AnyAsync(a => a.UserId == userId);

            var shouldBeDefault = request.IsDefault || !hasExistingAddresses;

            if (shouldBeDefault && hasExistingAddresses)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            var address = request.Adapt<ModelAddress>();
            address.Id = Guid.NewGuid();
            address.UserId = userId;
            address.IsDefault = shouldBeDefault;
            address.CreatedAt = DateTime.UtcNow;
            address.UpdatedAt = null;

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            var response = address.Adapt<AddressResponse>();
            response.CountryName = country.Name;
            response.StateName = state.Name;
            response.CityName = city.Name;

            return new ApiResponse<AddressResponse>
            {
                Success = true,
                Message = "Address created successfully.",
                Data = response,
                StatusCode = 201
            };
        }

        public async Task<ApiResponse<List<AddressResponse>>> GetAddressesByUserIdAsync(Guid userId)
        {
            var addresses = await _context.Addresses
                .AsNoTracking()
                .Include(a => a.Country)
                .Include(a => a.State)
                .Include(a => a.City)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            var response = addresses.Select(a =>
            {
                var dto = a.Adapt<AddressResponse>();
                dto.CountryName = a.Country?.Name ?? string.Empty;
                dto.StateName = a.State?.Name ?? string.Empty;
                dto.CityName = a.City?.Name ?? string.Empty;
                return dto;
            }).ToList();

            return new ApiResponse<List<AddressResponse>>
            {
                Success = true,
                Message = "Addresses retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<AddressResponse>> GetAddressByIdAsync(Guid userId, Guid id)
        {
            var address = await _context.Addresses
                .AsNoTracking()
                .Include(a => a.Country)
                .Include(a => a.State)
                .Include(a => a.City)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            var response = address.Adapt<AddressResponse>();
            response.CountryName = address.Country?.Name ?? string.Empty;
            response.StateName = address.State?.Name ?? string.Empty;
            response.CityName = address.City?.Name ?? string.Empty;

            return new ApiResponse<AddressResponse>
            {
                Success = true,
                Message = "Address retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<AddressResponse>> UpdateAddressAsync(Guid userId, Guid id, UpdateAddressRequest request)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            var country = await _context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CountryId && c.IsActive)
                ?? throw new NotFoundException("Selected country not found or is inactive.");

            var state = await _context.States
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.StateId && s.CountryId == request.CountryId && s.IsActive)
                ?? throw new BadRequestException("Selected state does not belong to the selected country or is inactive.");

            var city = await _context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CityId && c.StateId == request.StateId && c.IsActive)
                ?? throw new BadRequestException("Selected city does not belong to the selected state or is inactive.");

            if (request.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.Id != id && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }
            }

            request.Adapt(address);
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = address.Adapt<AddressResponse>();
            response.CountryName = country.Name;
            response.StateName = state.Name;
            response.CityName = city.Name;

            return new ApiResponse<AddressResponse>
            {
                Success = true,
                Message = "Address updated successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<bool>> DeleteAddressAsync(Guid userId, Guid id)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            if (address.IsDefault)
            {
                var nextAddress = await _context.Addresses
                    .Where(a => a.UserId == userId && a.Id != id)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                nextAddress?.IsDefault = true;
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Address deleted successfully.",
                Data = true,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<bool>> SetDefaultAddressAsync(Guid userId, Guid id)
        {
            var targetAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                ?? throw new NotFoundException("Address not found.");

            if (targetAddress.IsDefault)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Address is already set as default.",
                    Data = true,
                    StatusCode = 200
                };
            }

            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
            }

            targetAddress.IsDefault = true;
            targetAddress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Address set as default successfully.",
                Data = true,
                StatusCode = 200
            };
        }
    }
}

