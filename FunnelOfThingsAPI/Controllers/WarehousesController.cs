using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public WarehouseController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }


        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetWarehouses(int sellerId)
        {
            var warehouses = await _dbcontext.Warehouses
                .Where(w => w.SellerId == sellerId)
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Address,
                    w.CreatedAt,
                    StockCount = w.StockMovements.Count
                })
                .ToListAsync();

            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWarehouse(int id)
        {
            var warehouse = await _dbcontext.Warehouses
                .Include(w => w.StockMovements)
                    .ThenInclude(sm => sm.Product)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                return NotFound(new { message = "Склад не найден" });

            return Ok(new
            {
                warehouse.Id,
                warehouse.Name,
                warehouse.Address,
                warehouse.CreatedAt,
              
                movements = warehouse.StockMovements
                    .OrderByDescending(sm => sm.CreatedAt)
                    .Select(sm => new
                    {
                        sm.Id,
                        sm.Type,       
                        sm.Quantity,
                        sm.Comment,
                        sm.CreatedAt,
                        ProductName = sm.Product.Name
                    })
            });
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddWarehouse(
            [FromBody] WarehouseRequest request)
        {
            _dbcontext.Warehouses.Add(new Warehouse
            {
                SellerId = request.SellerId,
                Name = request.Name,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            });

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Склад добавлен" });
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateWarehouse(
            int id, [FromBody] WarehouseRequest request)
        {
            var warehouse = await _dbcontext.Warehouses.FindAsync(id);

            if (warehouse == null)
                return NotFound(new { message = "Склад не найден" });

            warehouse.Name = request.Name;
            warehouse.Address = request.Address;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Склад обновлён" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _dbcontext.Warehouses.FindAsync(id);

            if (warehouse == null)
                return NotFound(new { message = "Склад не найден" });

           
            var hasMovements = await _dbcontext.StockMovements
                .AnyAsync(sm => sm.WarehouseId == id);

            if (hasMovements)
                return BadRequest(new { message = "Нельзя удалить склад — есть история движения товаров" });

            _dbcontext.Warehouses.Remove(warehouse);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Склад удалён" });
        }

        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveStock(
            [FromBody] StockMovementRequest request)
        {
            var product = await _dbcontext.Products.FindAsync(request.ProductId);

            if (product == null)
                return NotFound(new { message = "Товар не найден" });

            _dbcontext.StockMovements.Add(new StockMovement
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Type = "receipt",
                Quantity = request.Quantity,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            });

            product.Stock += request.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();
            return Ok(new
            {
                message = "Товар принят на склад",
                newStock = product.Stock
            });
        }
    }

    public class WarehouseRequest
    {
        public int SellerId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
    }

    public class StockMovementRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public string? Comment { get; set; }
    }
}