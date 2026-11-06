using Ecommerce.Common;
using Ecommerce.Common.Exceptions;
using Ecommerce.Contracts;
using Ecommerce.Data;
using Ecommerce.DTO.Cart;
using Ecommerce.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CartResponse>> AddToCartAsync(Guid userId, AddToCartRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new BadRequestException("Quantity must be greater than zero.");
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId) ?? throw new NotFoundException("Product not found");

            if (!product.IsActive)
            {
                throw new NotFoundException("Product is currently unavailable.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Models.Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CartItems = []
                };
                await _context.Carts.AddAsync(cart);
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == request.ProductId);

            var totalRequestedQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

            if (product.StockQuantity < totalRequestedQuantity)
            {
                var currentInCart = existingItem?.Quantity ?? 0;
                string message = currentInCart > 0
                    ? $"Only {product.StockQuantity} available. You already have {currentInCart} in your cart."
                    : $"Only {product.StockQuantity} units of '{product.Name}' available.";
                throw new BadRequestException(message);
            }

            if (existingItem != null)
            {
                existingItem.Quantity = totalRequestedQuantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    Product = product
                };

                cart.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = MapToCartResponse(cart);

            return new ApiResponse<CartResponse>
            {
                Success = true,
                Message = "Item added to cart successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<CartResponse>> RemoveFromCartAsync(Guid userId, Guid productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? throw new NotFoundException("Cart not found.");

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId) ?? throw new NotFoundException("Product not found in cart.");

            cart.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            var response = MapToCartResponse(cart);

            return new ApiResponse<CartResponse>
            {
                Success = true,
                Message = "Item removed from cart successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<CartResponse>> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return new ApiResponse<CartResponse>
                {
                    Success = true,
                    Message = "Cart is empty.",
                    Data = new CartResponse { UserId = userId, Items = [] },
                    StatusCode = 200
                };
            }

            var response = MapToCartResponse(cart);
            return new ApiResponse<CartResponse>
            {
                Success = true,
                Message = "Cart retrieved successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<CartResponse>> UpdateCartItemQuantityAsync(Guid userId, UpdateCartItemRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new BadRequestException("Quantity must be greater than zero.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? throw new NotFoundException("Cart not found.");

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == request.ProductId) ?? throw new NotFoundException("Product not found in cart.");

            if (!cartItem.Product.IsActive)
            {
                throw new BadRequestException("This product is currently unavailable.");
            }

            if (cartItem.Product.StockQuantity < request.Quantity)
            {
                throw new BadRequestException($"Only {cartItem.Product.StockQuantity} unit(s) of '{cartItem.Product.Name}' are available in stock.");
            }

            cartItem.Quantity = request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = MapToCartResponse(cart);

            return new ApiResponse<CartResponse>
            {
                Success = true,
                Message = "Cart item quantity updated successfully.",
                Data = response,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? throw new NotFoundException("Cart not found.");

            if (cart.CartItems.Count == 0)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Cart is already empty.",
                    Data = true,
                    StatusCode = 200
                };
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Cart cleared successfully.",
                Data = true,
                StatusCode = 200
            };
        }

        #region Helper Methods
        private static CartResponse MapToCartResponse(Models.Cart cart)
        {
            var response = cart.Adapt<CartResponse>();

            response.Items = cart.CartItems.Select(ci =>
            {
                var item = ci.Adapt<CartItemResponse>();
                item.ProductName = ci.Product?.Name ?? string.Empty;
                item.ProductSlug = ci.Product?.Slug ?? string.Empty;
                item.ProductLogo = ci.Product?.Logo;
                item.UnitPrice = ci.Product?.Price ?? 0;
                item.DiscountPrice = ci.Product?.DiscountPrice;
                return item;
            }).ToList();

            return response;
        }

        #endregion
    }
}
