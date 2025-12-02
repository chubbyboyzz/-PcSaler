using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;
using System.Security.Claims; // Cần cái này để tạo định danh

namespace PcSaler.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _repo;
        private readonly IPasswordHasher _hasher;

        public AccountController(ILoginService repo, IPasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Check trùng Username
            var exists = await _repo.GetUsersByUsername(model.Username);
            if (exists != null)
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(model);
            }

            // 2. Hash Password
            string secureHash = _hasher.HashPassword(model.Password);

            // 3. Tạo User mới
            var newUser = new Customer
            {
                Username = model.Username,
                FullName = model.FullName,
                PasswordHash = secureHash,
                Email = model.Email,
                CreatedAt = DateTime.Now
            };

            // 4. Lưu vào DB (QUAN TRỌNG: Phải lưu xong mới có ID để tạo Cookie)
            await _repo.addAsync(newUser);
            await _repo.SaveChangeAsync(); // Lúc này newUser.CustomerID sẽ được DB tự sinh ra

            // ============================================================
            // 5. AUTO LOGIN (LOGIC MỚI THÊM VÀO)
            // ============================================================

            // Tạo thông tin định danh (Claims) - Copy y hệt logic bên LoginController
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.Username ?? ""),
                new Claim("FullName", newUser.FullName ?? "New Member"),
                // QUAN TRỌNG: Phải có ID này thì giỏ hàng mới biết của ai
                new Claim(ClaimTypes.NameIdentifier, newUser.CustomerID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Tự động nhớ đăng nhập luôn
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            // Ghi Cookie vào trình duyệt
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ============================================================

            // 6. Chuyển hướng thẳng về Trang chủ
            return RedirectToAction("Index", "Home");
        }
    }
}