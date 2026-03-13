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
    public class AddressController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public AddressController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAddresses(int userId)
        {
            var addresses = await _dbcontext.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    a.Id,
                    a.Label,
                    a.Country,
                    a.City,
                    a.Street,
                    a.Building,
                    a.Apartment,
                    a.PostalCode,
                    a.IsDefault
                })
                .ToListAsync();

            return Ok(addresses);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAddress(
            [FromBody] AddAddressRequest request)
        {

            if (request.IsDefault)
            {
                var existing = await _dbcontext.Addresses
                    .Where(a => a.UserId == request.UserId && a.IsDefault)
                    .ToListAsync();

                foreach (var addr in existing)
                {
                    addr.IsDefault = false;
                    addr.UpdatedAt = DateTime.UtcNow;
                }
            }

            _dbcontext.Addresses.Add(new Address
            {
                UserId = request.UserId,
                Label = request.Label,
                Country = request.Country,
                City = request.City,
                Street = request.Street,
                Building = request.Building,
                Apartment = request.Apartment,
                PostalCode = request.PostalCode,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow
            });

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Адрес добавлен" });
        }

        [HttpPut("setdefault")]
        public async Task<IActionResult> SetDefault(
            [FromBody] SetDefaultRequest request)
        {
            
            var allAddresses = await _dbcontext.Addresses
                .Where(a => a.UserId == request.UserId)
                .ToListAsync();

            foreach (var addr in allAddresses)
            {
                addr.IsDefault = addr.Id == request.AddressId;
                addr.UpdatedAt = DateTime.UtcNow;
            }

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Основной адрес обновлён" });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAddress(
            int id, [FromBody] AddAddressRequest request)
        {
            var address = await _dbcontext.Addresses.FindAsync(id);

            if (address == null)
                return NotFound(new { message = "Адрес не найден" });

            address.Label = request.Label;
            address.Country = request.Country;
            address.City = request.City;
            address.Street = request.Street;
            address.Building = request.Building;
            address.Apartment = request.Apartment;
            address.PostalCode = request.PostalCode;
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Адрес обновлён" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _dbcontext.Addresses.FindAsync(id);

            if (address == null)
                return NotFound(new { message = "Адрес не найден" });

           
            var hasOrders = await _dbcontext.Orders
                .AnyAsync(o => o.AddressId == id);

            if (hasOrders)
                return BadRequest(new { message = "Нельзя удалить адрес — к нему привязаны заказы" });

            _dbcontext.Addresses.Remove(address);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Адрес удалён" });
        }
    }

    public class AddAddressRequest
    {
        public int UserId { get; set; }
        public string? Label { get; set; }       
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? Building { get; set; }
        public string? Apartment { get; set; }
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class SetDefaultRequest
    {
        public int UserId { get; set; }
        public int AddressId { get; set; }
    }
}