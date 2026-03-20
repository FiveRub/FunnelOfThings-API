using FunnelOfThingsAPI.Data;
using FunnelOfThingsAPI.Models;
using FunnelOfThingsAPI.Transfer.Requests;
using FunnelOfThingsAPI.Transfer.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunnelOfThingsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SellerController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/Seller/profile/5 — профиль продавца по userId
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var profile = await _db.SellerProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Профиль продавца не найден" });

            return Ok(MapToResponse(profile));
        }

        // POST api/Seller/register — регистрация юр. лица
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterSellerRequest request)
        {
            // Проверяем что пользователь существует
            var user = await _db.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            // Проверяем что профиль ещё не создан
            var exists = await _db.SellerProfiles
                .AnyAsync(s => s.UserId == request.UserId);
            if (exists)
                return BadRequest(new { message = "Профиль продавца уже существует" });

            // Проверяем уникальность БИН/ИНН
            var binExists = await _db.SellerProfiles
                .AnyAsync(s => s.BinIin == request.BinIin);
            if (binExists)
                return BadRequest(new { message = "БИН/ИНН уже зарегистрирован" });

            var profile = new SellerProfile
            {
                UserId = request.UserId,
                CompanyName = request.CompanyName,
                BinIin = request.BinIin,
                LegalAddress = request.LegalAddress,
                BankAccount = request.BankAccount,
                BankName = request.BankName,
                Bik = request.Bik,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.SellerProfiles.Add(profile);

            // Назначаем роль seller если её нет
            var sellerRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name == "seller");

            if (sellerRole != null)
            {
                var hasRole = await _db.UserRoles
                    .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == sellerRole.Id);

                if (!hasRole)
                {
                    _db.UserRoles.Add(new UserRole
                    {
                        UserId = request.UserId,
                        RoleId = sellerRole.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(MapToResponse(profile));
        }

        // PUT api/Seller/profile/5 — обновить профиль
        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateSellerRequest request)
        {
            var profile = await _db.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Профиль продавца не найден" });

            profile.CompanyName = request.CompanyName ?? profile.CompanyName;
            profile.LegalAddress = request.LegalAddress ?? profile.LegalAddress;
            profile.BankAccount = request.BankAccount ?? profile.BankAccount;
            profile.BankName = request.BankName ?? profile.BankName;
            profile.Bik = request.Bik ?? profile.Bik;
            profile.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(MapToResponse(profile));
        }

        // GET api/Seller/products/5 — товары продавца
        [HttpGet("products/{userId}")]
        public async Task<IActionResult> GetProducts(int userId)
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.SellerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.IsActive,
                p.ModerationStatus,
                Category = p.Category.Name,
                MainImageUrl = p.ProductImages.FirstOrDefault(i => i.IsMain)?.Url
            }));
        }

        // GET api/Seller/payouts/5 — выплаты продавца
        [HttpGet("payouts/{userId}")]
        public async Task<IActionResult> GetPayouts(int userId)
        {
            var payouts = await _db.SellerPayouts
                .Include(p => p.Order)
                .Where(p => p.SellerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(payouts.Select(p => new SellerPayoutResponse
            {
                Id = p.Id,
                OrderId = p.OrderId,
                GrossAmount = p.GrossAmount,
                CommissionPct = p.CommissionPct,
                NetAmount = p.NetAmount,
                Status = p.Status,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            }));
        }

        // GET api/Seller/stats/5 — статистика продавца
        [HttpGet("stats/{userId}")]
        public async Task<IActionResult> GetStats(int userId)
        {
            var totalProducts = await _db.Products
                .CountAsync(p => p.SellerId == userId && p.IsActive);

            var totalOrders = await _db.SellerPayouts
                .CountAsync(p => p.SellerId == userId);

            var totalRevenue = await _db.SellerPayouts
                .Where(p => p.SellerId == userId && p.Status == "paid")
                .SumAsync(p => (decimal?)p.NetAmount) ?? 0;

            var pendingPayouts = await _db.SellerPayouts
                .Where(p => p.SellerId == userId && p.Status == "pending")
                .SumAsync(p => (decimal?)p.NetAmount) ?? 0;

            return Ok(new
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                PendingPayouts = pendingPayouts
            });
        }

        // PUT api/Seller/approve/5 — одобрить продавца (для админа)
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var profile = await _db.SellerProfiles.FindAsync(id);

            if (profile == null)
                return NotFound(new { message = "Профиль не найден" });

            profile.Status = "approved";
            profile.ApprovedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Продавец одобрен" });
        }

        // PUT api/Seller/reject/5 — отклонить продавца (для админа)
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var profile = await _db.SellerProfiles.FindAsync(id);

            if (profile == null)
                return NotFound(new { message = "Профиль не найден" });

            profile.Status = "rejected";
            profile.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Продавец отклонён" });
        }

  
        private static SellerProfileResponse MapToResponse(SellerProfile p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            CompanyName = p.CompanyName,
            BinIin = p.BinIin,
            LegalAddress = p.LegalAddress,
            BankAccount = p.BankAccount,
            BankName = p.BankName,
            Bik = p.Bik,
            Status = p.Status,
            ApprovedAt = p.ApprovedAt,
            CreatedAt = p.CreatedAt
        };
    }
}