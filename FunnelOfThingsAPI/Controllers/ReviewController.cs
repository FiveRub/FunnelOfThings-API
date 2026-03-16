using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace FunnelOfThingsAPI.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public ReviewController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }


        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _dbcontext.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    AuthorName = r.User.FirstName + " " + r.User.LastName
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

  
            var avgRating = reviews.Any()
                ? Math.Round(reviews.Average(r => (double)r.Rating), 1)
                : 0;

            return Ok(new
            {
                averageRating = avgRating,
                totalCount = reviews.Count,
                reviews
            });
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserReviews(int userId)
        {
            var reviews = await _dbcontext.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    ProductName = r.Product.Name,
                    r.ProductId
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reviews);
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddReview(
            [FromBody] AddReviewRequest request)
        {
            var exists = await _dbcontext.Reviews
                .AnyAsync(r => r.UserId == request.UserId
                            && r.ProductId == request.ProductId);

            if (exists)
                return BadRequest(new { message = "Вы уже оставили отзыв на этот товар" });

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Рейтинг должен быть от 1 до 5" });

           
            if (request.OrderId.HasValue)
            {
                var orderExists = await _dbcontext.Orders
                    .AnyAsync(o => o.Id == request.OrderId
                                && o.UserId == request.UserId);

                if (!orderExists)
                    return BadRequest(new { message = "Заказ не найден" });
            }

            _dbcontext.Reviews.Add(new Review
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            });

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Отзыв добавлен" });
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateReview(
            int id, [FromBody] UpdateReviewRequest request)
        {
            var review = await _dbcontext.Reviews.FindAsync(id);

            if (review == null)
                return NotFound(new { message = "Отзыв не найден" });

            
            if (review.UserId != request.UserId)
                return BadRequest(new { message = "Нельзя редактировать чужой отзыв" });

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Рейтинг должен быть от 1 до 5" });

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Отзыв обновлён" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _dbcontext.Reviews.FindAsync(id);

            if (review == null)
                return NotFound(new { message = "Отзыв не найден" });

            _dbcontext.Reviews.Remove(review);
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Отзыв удалён" });
        }
    }
    public class AddReviewRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int? OrderId { get; set; }   
        public byte Rating { get; set; }    
        public string? Comment { get; set; }
    }

    public class UpdateReviewRequest
    {
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
    }
}