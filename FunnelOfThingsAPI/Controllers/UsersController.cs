using Microsoft.AspNetCore.Mvc;
using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using FunnelOfThingsAPI.Services;
using FunnelOfThingsAPI.Transfer.Requests;
using FunnelOfThingsAPI.Transfer.Responses;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;

        public UsersController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _dbcontext.Users.
                FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });
            return Ok(new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            }
                );
        }

        // POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>/5

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _dbcontext.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Phone = request.Phone ?? user.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();

            return Ok( new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt

            });
        }

        [HttpPut("{id}/avatar")]
        public async Task<IActionResult> UpdateAvatar(int id, [FromBody] string avatarUrl)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            user.AvatarUrl = avatarUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Аватар обновлён" ,  avatarUrl = user.AvatarUrl });
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Пользователь деактивирован" });

        }
    }
}
