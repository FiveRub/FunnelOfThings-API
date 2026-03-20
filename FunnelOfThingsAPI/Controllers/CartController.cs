using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public CartController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var cart = await _dbcontext.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new { cartId = 0, items = new List<object>(), total = 0 });

            var items = cart.CartItems.Select(ci => new
            {
                ci.Id,
                ci.ProductId,
                ProductName = ci.Product.Name,
                ProductPrice = ci.Product.Price,
                ci.Quantity,
                Subtotal = ci.Product.Price * ci.Quantity,
                MainImageUrl = ci.Product.ProductImages
                    .Where(pi => pi.IsMain)
                    .Select(pi => baseUrl + pi.Url)
                    .FirstOrDefault()
            });

            return Ok(new
            {
                cartId = cart.Id,
                items,
                total = items.Sum(i => i.Subtotal)
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var cart = await _dbcontext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbcontext.Carts.Add(cart);
                await _dbcontext.SaveChangesAsync();
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbcontext.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Товар добавлен в корзину" });
        }

        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] int quantity)
        {
            var item = await _dbcontext.CartItems.FindAsync(cartItemId);

            if (item == null)
                return NotFound(new { message = "Товар не найден в корзине" });

            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Количество обновлено" });
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var item = await _dbcontext.CartItems.FindAsync(cartItemId);

            if (item == null)
                return NotFound(new { message = "Товар не найден" });

            _dbcontext.CartItems.Remove(item);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Товар удалён из корзины" });
        }

        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var cart = await _dbcontext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound(new { message = "Корзина не найдена" });

            _dbcontext.CartItems.RemoveRange(cart.CartItems);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Корзина очищена" });
        }
    }

    public class AddToCartRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}