using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google; // Thư viện Google
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext.Entites;
using PcSaler.Models;
using PcSaler.Services;
using System.Security.Claims;

namespace PcSaler.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string returnUrl = "/")
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model, string returnUrl = "/")
        {
            if (ModelState.IsValid)
            {
                var user = await _loginService.LoginUserAsync(model.Username, model.Password);

                if (user == null)
                {
                    ViewData["MessageLogin"] = "Tài khoản hoặc mật khẩu không đúng.";
                    return View(model);
                }

                await SignInUser(user); // Gọi hàm đăng nhập chung

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/")
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // ==========================================
        //  LOGIC GOOGLE LOGIN (Mới thêm vào)
        // ==========================================

        [AllowAnonymous]
        public IActionResult LoginByGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded) return RedirectToAction("Index", "Home");

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Index", "Home");

            // --- CHECK USER TRONG DB ---
            // Lưu ý: Đảm bảo LoginService của ông đã có hàm GetUsersByEmail 
            // Nếu chưa có, ông phải vào Repository_Login thêm vào nhé
            // Ở đây tui giả định ông dùng Repository thông qua Service, hoặc ông có thể inject Repository trực tiếp


            // UPDATE: Để code này chạy ngay mà không cần sửa Service nhiều, 
            
            // Nhưng để đơn giản nhất cho ông, tui giả định ông đã thêm hàm GetUsersByEmail vào LoginService rồi.

            // Cách fix nhanh: Nếu LoginService chưa có hàm này, ông mở LoginService.cs thêm hàm gọi xuống Repo nhé.
            var user = await _loginService.GetUsersByEmail(email);

            if (user == null)
            {
                // TỰ ĐỘNG ĐĂNG KÝ
                user = new Customer
                {
                    Username = email,
                    Email = email,
                    FullName = name,
                    CreatedAt = DateTime.Now,
                    PasswordHash = "GOOGLE_AUTH_NO_PASSWORD", // Google user không có pass
                    Address = "Chưa cập nhật",
                    Phone = ""
                };

                await _loginService.addAsync(user);
                await _loginService.SaveChangeAsync();
            }

            // ĐĂNG NHẬP
            await SignInUser(user);

            return RedirectToAction("Index", "Home");
        }

        // Hàm phụ trợ để tái sử dụng logic tạo Cookie
        private async Task SignInUser(Customer user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim("FullName", user.FullName ?? "Khách hàng"),
                new Claim(ClaimTypes.NameIdentifier, user.CustomerID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}