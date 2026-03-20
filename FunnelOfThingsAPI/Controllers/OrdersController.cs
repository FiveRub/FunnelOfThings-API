using FunnelOfThingsAPI.Data;
using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Transfer.Requests;
using FunnelOfThingsAPI.Models;
using FunnelOfThingsAPI.Transfer.Responses;
using Microsoft.EntityFrameworkCore;

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public OrdersController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // GET api/orders/detail/5
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _dbcontext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Заказ не найден" });

            return Ok(MapToOrderResponse(order));
        }

       
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetOrders(int userId)
        {
            var orders = await _dbcontext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            if (!orders.Any())
            {
                return Ok(new List<OrderResponse>());
            }

            return Ok(orders.Select(MapToOrderResponse));
        }

        // POST api/orders/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var cart = await _dbcontext.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId);

            if (cart == null || cart.CartItems.Count == 0)
                return BadRequest(new { message = "Cart is empty" });

            var address = await _dbcontext.Addresses
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId);

            if (address == null)
                return BadRequest(new { message = "Invalid address" });

            var totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            var order = new Order
            {
                UserId = request.UserId,
                AddressId = request.AddressId,
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _dbcontext.Orders.Add(order);
            await _dbcontext.SaveChangesAsync();

            var orderItems = cart.CartItems.Select(item => new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _dbcontext.OrderItems.AddRange(orderItems);

            var responseItems = cart.CartItems.Select(ci => new OrderItemResponse
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Quantity = ci.Quantity,
                Price = (float)ci.Product.Price
            }).ToList();

            _dbcontext.CartItems.RemoveRange(cart.CartItems);
            await _dbcontext.SaveChangesAsync();

            return Ok(new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                OrderItems = responseItems 
            });
        }

        // PUT api/orders/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _dbcontext.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            
            if (order.Status != "Pending")
                return BadRequest(new { message = "Cannot cancel order with status: " + order.Status });

            order.Status = "Cancelled";
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Order cancelled" });
        }

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _dbcontext.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            _dbcontext.Orders.Remove(order);
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Order deleted" });
        }


        private static OrderResponse MapToOrderResponse(Order order) => new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            AddressId = order.AddressId,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                Price = (float)oi.UnitPrice
            }).ToList()
        };
    }
}