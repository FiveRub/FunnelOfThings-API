using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;



namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public ProductsController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }


        // GET: api/<ProductsController>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? categoryId = null)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var query = _dbcontext.Products
                .Where(p => p.IsActive == true);

            // Фильтр по категории если передан
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var products = await query
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    MainImageUrl = p.ProductImages
                        .Where(pi => pi.IsMain)
                        .Select(pi => pi.Url)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var result = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                MainImageUrl = p.MainImageUrl != null ? baseUrl + p.MainImageUrl : null
            });

            return Ok(result);
        }


        // POST api/<ProductsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ProductsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProductsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
