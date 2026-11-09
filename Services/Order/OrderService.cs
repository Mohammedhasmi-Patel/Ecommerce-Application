using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Order;
using Ecommerce.Enum;
using Ecommerce.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ModelOrder = Ecommerce.Models.Order;

namespace Ecommerce.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new BadRequestException("Cannot create an order from an empty cart.");
            }

            var address = await _context.Addresses
                .AsNoTracking()
                .Include(a => a.Country)
                .Include(a => a.State)
                .Include(a => a.City)
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId)
                ?? throw new NotFoundException("Shipping address not found.");

            foreach (var item in cart.CartItems)
            {
                if (!item.Product.IsActive)
                {
                    throw new BadRequestException($"Product '{item.Product.Name}' is no longer available.");
                }

                if (item.Product.StockQuantity < item.Quantity)
                {
                    throw new BadRequestException($"Only {item.Product.StockQuantity} unit(s) of '{item.Product.Name}' available in stock.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();

                var order = new ModelOrder
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderNumber = orderNumber,
                    OrderStatus = OrderStatusEnum.Pending,
                    PaymentStatus = PaymentStatusEnum.Pending,
                    Notes = request.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var orderAddress = new OrderAddress
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    FullName = address.FullName,
                    PhoneNumber = address.PhoneNumber,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    Country = address.Country?.Name ?? string.Empty,
                    State = address.State?.Name ?? string.Empty,
                    City = address.City?.Name ?? string.Empty,
                    PostalCode = address.PostalCode,
                    Landmark = address.Landmark,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderAddress = orderAddress;

                foreach (var cartItem in cart.CartItems)
                {
                    var unitPrice = cartItem.Product.DiscountPrice ?? cartItem.Product.Price;
                    var lineTotal = unitPrice * cartItem.Quantity;
                    subTotal += lineTotal;

                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                    cartItem.Product.UpdatedAt = DateTime.UtcNow;

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        UnitPrice = unitPrice,
                        Quantity = cartItem.Quantity,
                        TotalPrice = lineTotal,
                        CreatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);
                }

                order.OrderItems = orderItems;
                order.SubTotal = subTotal;
                order.DiscountAmount = 0;
                order.ShippingAmount = 0;
                order.TaxAmount = 0;
                order.TotalAmount = subTotal;

                await _context.Orders.AddAsync(order);

                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = MapToOrderResponse(order);

                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order placed successfully.",
                    Data = response,
                    StatusCode = 201
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.OrderAddress)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            var response = MapToOrderResponse(order);

            return new ApiResponse<OrderResponse>
            {
                Success = true,
                Message = "Order fetched successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<OrderResponse>>> GetMyOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.OrderItems)
                .Include(o => o.OrderAddress)
                .ToListAsync();

            var orderListRes = orders.Select(MapToOrderResponse).ToList();
            return new ApiResponse<List<OrderResponse>>
            {
                Success = true,
                Message = "Orders fetched successfully.",
                Data = orderListRes,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<OrderResponse>> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderAddress)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            if (order.OrderStatus == OrderStatusEnum.Cancelled)
            {
                throw new BadRequestException("Order is already cancelled.");
            }

            if (order.OrderStatus != OrderStatusEnum.Pending)
            {
                throw new BadRequestException($"Cannot cancel order with status '{order.OrderStatus}'. Only pending orders can be cancelled.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                order.OrderStatus = OrderStatusEnum.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;

                if (order.PaymentStatus == PaymentStatusEnum.Paid)
                {
                    order.PaymentStatus = PaymentStatusEnum.Refunded;
                }

                var productIds = order.OrderItems.Select(oi => oi.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var item in order.OrderItems)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = MapToOrderResponse(order);

                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order cancelled successfully.",
                    Data = response,
                    StatusCode = 200
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse<List<OrderResponse>>> GetAllOrdersAdminAsync(GetOrdersAdminRequest request)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderAddress)
                .Include(o => o.OrderItems)
                .AsQueryable();

            query = ApplyOrderFilters(query, request);

            query = query.OrderByDescending(o => o.CreatedAt);

            if (request.Page > 0 && request.PageSize > 0)
            {
                query = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);
            }

            var orders = await query.ToListAsync();
            var response = orders.Select(MapToOrderResponse).ToList();

            return new ApiResponse<List<OrderResponse>>
            {
                Success = true,
                Message = "Orders retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<OrderResponse>> GetOrderByIdAdminAsync(Guid orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderAddress)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            var response = MapToOrderResponse(order);

            return new ApiResponse<OrderResponse>
            {
                Success = true,
                Message = "Order details retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.OrderAddress)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            if (order.OrderStatus == request.OrderStatus)
            {
                throw new BadRequestException($"Order is already in '{request.OrderStatus}' status.");
            }

            if (order.OrderStatus == OrderStatusEnum.Cancelled)
            {
                throw new BadRequestException("Cannot update status of an already cancelled order.");
            }

            if (order.OrderStatus == OrderStatusEnum.Delivered && request.OrderStatus != OrderStatusEnum.Returned)
            {
                throw new BadRequestException("Delivered order can only be transitioned to 'Returned' status.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var previousStatus = order.OrderStatus;
                order.OrderStatus = request.OrderStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // If moving to Cancelled or Returned, restore inventory and refund if paid
                if (request.OrderStatus == OrderStatusEnum.Cancelled || request.OrderStatus == OrderStatusEnum.Returned)
                {
                    if (order.PaymentStatus == PaymentStatusEnum.Paid)
                    {
                        order.PaymentStatus = PaymentStatusEnum.Refunded;
                    }

                    var productIds = order.OrderItems.Select(oi => oi.ProductId).Distinct().ToList();
                    var products = await _context.Products
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync();

                    foreach (var item in order.OrderItems)
                    {
                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            product.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
                else if (request.OrderStatus == OrderStatusEnum.Delivered && order.PaymentStatus == PaymentStatusEnum.Pending)
                {
                    order.PaymentStatus = PaymentStatusEnum.Paid;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = MapToOrderResponse(order);

                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = $"Order status updated from '{previousStatus}' to '{order.OrderStatus}' successfully.",
                    Data = response,
                    StatusCode = 200
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #region Helper Methods

        private static IQueryable<ModelOrder> ApplyOrderFilters(IQueryable<ModelOrder> query, GetOrdersAdminRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(search) ||
                    (o.User != null && (
                        o.User.FirstName.ToLower().Contains(search) ||
                        (o.User.LastName != null && o.User.LastName.ToLower().Contains(search)) ||
                        (o.User.Email != null && o.User.Email.ToLower().Contains(search))
                    )) ||
                    (o.OrderAddress != null && o.OrderAddress.FullName.ToLower().Contains(search))
                );
            }

            if (request.OrderStatus.HasValue)
            {
                query = query.Where(o => o.OrderStatus == request.OrderStatus.Value);
            }

            if (request.PaymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == request.PaymentStatus.Value);
            }

            if (request.FromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(request.FromDate.Value, DateTimeKind.Utc);
                query = query.Where(o => o.CreatedAt >= fromUtc);
            }

            if (request.ToDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(request.ToDate.Value, DateTimeKind.Utc);
                query = query.Where(o => o.CreatedAt <= toUtc);
            }

            return query;
        }

        private static OrderResponse MapToOrderResponse(ModelOrder order)
        {
            var response = order.Adapt<OrderResponse>();

            if (order.User != null)
            {
                response.CustomerName = $"{order.User.FirstName} {order.User.LastName}".Trim();
                response.CustomerEmail = order.User.Email;
            }

            if (order.OrderAddress != null)
            {
                response.ShippingAddress = order.OrderAddress.Adapt<OrderAddressResponse>();
                if (string.IsNullOrEmpty(response.CustomerName))
                {
                    response.CustomerName = order.OrderAddress.FullName;
                }
            }

            response.Items = order.OrderItems.Select(oi => oi.Adapt<OrderItemResponse>()).ToList();

            return response;
        }

        #endregion
    }
}

