using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using FunnelOfThingsAPI.Services;
using FunnelOfThingsAPI.Transfer.Requests;
using FunnelOfThingsAPI.Transfer.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _config;

        public AuthorizationController(
            AppDbContext dbcontext,
            PasswordService passwordService,
            IConfiguration config) 
        {
            _dbcontext = dbcontext;
            _passwordService = passwordService;
            _config = config;
        }

        // POST api/Authorization/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {

            var existingUser = await _dbcontext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email уже занят" });

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                IsVerified = false,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _dbcontext.Users.Add(user);
            await _dbcontext.SaveChangesAsync();


            var buyerRole = await _dbcontext.Roles
                .FirstOrDefaultAsync(r => r.Name == "buyer");
            if (buyerRole != null)
            {
                _dbcontext.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = buyerRole.Id,
                    CreatedAt = DateTime.UtcNow
                });
                await _dbcontext.SaveChangesAsync();
            }

            var token = GenerateToken(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Token = token
            });
        }

        // POST api/Authorization/login
        [HttpPost("login")] // ← добавил атрибут
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _dbcontext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Неверный email или пароль" });

            if (!user.IsActive)
                return Unauthorized(new { message = "Аккаунт заблокирован" });

            var token = GenerateToken(user);

            return Ok(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Token = token
            });
        }

        private string GenerateToken(User user)
        {
        
            var secret = _config["Jwt:Secret"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("email",  user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}