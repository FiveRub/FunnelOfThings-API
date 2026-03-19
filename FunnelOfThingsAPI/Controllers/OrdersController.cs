using FunnelOfThingsAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Transfer.Requests;
using FunnelOfThingsAPI.Models;
using FunnelOfThingsAPI.Transfer.Responses;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        // GET: api/<OrdersController>
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _dbcontext.Orders.Include(o=> o.OrderItems)
                .ThenInclude(oi=> oi.Product)
                .FirstOrDefaultAsync(o=> o.Id == id);
            if (order == null)
            {
                return NotFound(new { message = "Order not include"});
            }



            return Ok(new OrderResponse 
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(oi=> new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = (float)oi.UnitPrice,


                }).ToList()


            });
        }


        // GET api/<OrdersController>/5
        [HttpGet("{userid}")]
        public async Task<IActionResult> GetOrders(int userid)
        {
            var orders = await _dbcontext.Orders.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o=> o.UserId==userid)
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound("Order not found");
            }

            return Ok(orders.Select( order=>new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = (float)oi.UnitPrice
                }).ToList()
            }));

        }

        // POST api/<OrdersController>
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var card = await _dbcontext.Carts.Include(c => c.CartItems)
                .ThenInclude(ci=> ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId);

            if (card == null || card.CartItems.Count == 0)
            {
                return BadRequest("Cart is empty");
            }

            var address = await _dbcontext.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId);

            if (address == null)
            {
                return BadRequest("Invalid address");
            }

            var totalPrice = card.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

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

            foreach (var item in card.CartItems)
            {
                _dbcontext.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    CreatedAt = DateTime.UtcNow
                });
               


            }

            _dbcontext.CartItems.RemoveRange(card.CartItems);

            await _dbcontext.SaveChangesAsync();





            return Ok(new OrderResponse 
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                OrderItems = card.CartItems.Select(ci => new OrderItemResponse
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    Price = (float)ci.Product.Price
                }).ToList()


            });

        }

        // PUT api/<OrdersController>/5
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _dbcontext.Orders.FindAsync(id);

            if (order == null)
            {
                return BadRequest();

            }

            if (order.Status != "pending")
            {
                return BadRequest(new { message ="Cannot cancel order"});
            }

            order.Status = "cancelled";
            //order.UpdateAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Order canceled" });

        }

        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
