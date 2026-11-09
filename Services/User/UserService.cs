using Ecommerce.Common;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.User;
using Ecommerce.Enum;
using Ecommerce.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AspNetUsers> _userManager;

        public UserService(AppDbContext context, UserManager<AspNetUsers> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApiResponse<List<UserAdminResponse>>> GetAllUsersAdminAsync(GetUsersAdminRequest request)
        {
            var query = _context.Users.AsNoTracking();

            query = ApplyUserFilters(query, request);

            query = query.OrderByDescending(u => u.CreatedAt);

            if (request.Page > 0 && request.PageSize > 0)
            {
                query = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);
            }

            var users = await query.ToListAsync();

            if (users.Count == 0)
            {
                return new ApiResponse<List<UserAdminResponse>>
                {
                    Success = true,
                    Message = "No users found.",
                    Data = [],
                    StatusCode = 200
                };
            }

            var userIds = users.Select(u => u.Id).ToList();

            var orderStats = await _context.Orders
                .Where(o => userIds.Contains(o.UserId))
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    OrdersCount = g.Count(),
                    TotalSpent = g.Where(o => o.PaymentStatus == PaymentStatusEnum.Paid).Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            var response = users.Select(user =>
            {
                var userDto = user.Adapt<UserAdminResponse>();
                userDto.Role = nameof(UserRoleEnum.User);

                var stats = orderStats.FirstOrDefault(s => s.UserId == user.Id);
                userDto.OrdersCount = stats?.OrdersCount ?? 0;
                userDto.TotalSpent = stats?.TotalSpent ?? 0;

                userDto.IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                return userDto;
            }).ToList();

            return new ApiResponse<List<UserAdminResponse>>
            {
                Success = true,
                Message = "Users retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        #region Helper Methods

        private IQueryable<AspNetUsers> ApplyUserFilters(IQueryable<AspNetUsers> query, GetUsersAdminRequest request)
        {
            var userRoleName = nameof(UserRoleEnum.User).ToUpper();
            var customerUserIds = from ur in _context.UserRoles
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where r.NormalizedName == userRoleName
                                  select ur.UserId;

            query = query.Where(u => customerUserIds.Contains(u.Id));

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
                );
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            return query;
        }

        #endregion
    }
}

