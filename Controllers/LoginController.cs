using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Services;
using System.Security.Claims;

namespace PcSaler.Controllers
{
    public class LoginController : Controller
    {
        // Controller chỉ làm việc với Interface Service, không biết gì về Database
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string returnUrl = "/")
        {
            // Nếu người dùng đã đăng nhập rồi mà quay lại trang login, 
            // ta tự động đăng xuất để họ đăng nhập tài khoản mới (hoặc có thể redirect về Home tùy logic bạn muốn)
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
                // 1. Gọi Service để kiểm tra thông tin đăng nhập
                // Logic check pass, hash, query DB nằm hết trong Service này
                var user = await _loginService.LoginUserAsync(model.Username, model.Password);

                if (user == null)
                {
                    ViewData["MessageLogin"] = "Tài khoản hoặc mật khẩu không đúng.";
                    return View(model);
                }

                // 2. Đăng nhập thành công -> Tạo thông tin định danh (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username ?? ""),
                    new Claim("FullName", user.FullName ?? "Khách hàng"),
                    
                    // QUAN TRỌNG: Lưu CustomerID vào Claim NameIdentifier
                    // APICartController sẽ tìm claim này để biết giỏ hàng thuộc về ai
                    new Claim(ClaimTypes.NameIdentifier, user.CustomerID.ToString())
                };

                // Tạo Identity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Cấu hình Cookie (Giữ đăng nhập 7 ngày)
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                };

                // 3. Ghi Cookie vào trình duyệt (Chính thức đăng nhập)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 4. Chuyển hướng về trang trước đó (nếu có) hoặc về trang chủ
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/")
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa Session nếu có dùng
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}