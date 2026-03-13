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
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public FavoriteController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            var favorites = await _dbcontext.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                    .ThenInclude(p => p.ProductImages)
                .Select(f => new
                {
                    f.Id,
                    f.ProductId,
                    ProductName = f.Product.Name,
                    ProductPrice = f.Product.Price,
                    MainImageUrl = f.Product.ProductImages
                        .Where(pi => pi.IsMain)
                        .Select(pi => pi.Url)
                        .FirstOrDefault(),
                    f.CreatedAt
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToFavorite(
            [FromBody] AddToFavoriteRequest request)
        {
            
            var exists = await _dbcontext.Favorites
                .AnyAsync(f => f.UserId == request.UserId
                            && f.ProductId == request.ProductId);

            if (exists)
                return Ok(new { message = "Товар уже в избранном" });

            _dbcontext.Favorites.Add(new Favorite
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                CreatedAt = DateTime.UtcNow
            });

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Товар добавлен в избранное" });
        }
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromFavorite(
            [FromBody] AddToFavoriteRequest request)
        {
            var favorite = await _dbcontext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId
                                       && f.ProductId == request.ProductId);

            if (favorite == null)
                return NotFound(new { message = "Товар не найден в избранном" });

            _dbcontext.Favorites.Remove(favorite);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Товар удалён из избранного" });
        }

     
        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearFavorites(int userId)
        {
            var favorites = await _dbcontext.Favorites
                .Where(f => f.UserId == userId)
                .ToListAsync();

            if (!favorites.Any())
                return NotFound(new { message = "Избранное уже пустое" });

            _dbcontext.Favorites.RemoveRange(favorites);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Избранное очищено" });
        }
    }
    public class AddToFavoriteRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}