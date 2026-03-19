using FunnelOfThingsAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _db.Categories
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Name, c.Slug, c.ParentId })
                .ToListAsync();
            return Ok(categories);
        }
    }
}