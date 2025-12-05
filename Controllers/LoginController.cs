using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext.Entites;
using PcSaler.Models;
using PcSaler.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PcSaler.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        #region 1. XỬ LÝ ĐĂNG NHẬP & ĐĂNG XUẤT (STANDARD)

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string returnUrl = "/")
        {
            // Nếu đã login rồi thì logout phiên cũ để tránh xung đột
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model, string returnUrl = "/")
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            // Gọi Service check user/pass
            var user = await _loginService.LoginUserAsync(model.Username, model.Password);

            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không đúng." });
            }

            // Ghi Cookie đăng nhập
            await SignInUser(user);

            // Kiểm tra URL redirect an toàn
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl) || returnUrl == "/")
            {
                returnUrl = Url.Action("Index", "Home");
            }

            return Json(new { success = true, redirectUrl = returnUrl });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region 2. XỬ LÝ QUÊN MẬT KHẨU (FORGOT PASSWORD API)

        // Bước 0: Kiểm tra Username (Cho flow tìm bằng Tên đăng nhập)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserInfo(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Json(new { success = false, message = "Vui lòng nhập tên đăng nhập!" });

            var user = await _loginService.GetUsersByUsername(username);

            if (user == null)
                return Json(new { success = false, message = "Tên đăng nhập không tồn tại." });

            if (string.IsNullOrEmpty(user.Email))
                return Json(new { success = false, message = "Tài khoản này chưa cập nhật Email." });

            return Json(new
            {
                success = true,
                fullName = user.FullName ?? user.Username,
                maskedEmail = MaskEmail(user.Email) // Trả về email đã che
            });
        }

        // Bước 1A: Gửi OTP (Theo Email trực tiếp)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Vui lòng nhập Email!" });

            var result = await _loginService.SendForgotPasswordEmailAsync(email);

            return result
                ? Json(new { success = true, message = "Mã OTP đã được gửi đến email." })
                : Json(new { success = false, message = "Email này chưa đăng ký." });
        }

        // Bước 1B: Gửi OTP (Theo Username - Tự tìm Email để gửi)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtpByUsername(string username)
        {
            var user = await _loginService.GetUsersByUsername(username);
            if (user == null) return Json(new { success = false, message = "Lỗi hệ thống." });

            var result = await _loginService.SendForgotPasswordEmailAsync(user.Email);

            return result
                ? Json(new { success = true, message = "Đã gửi mã OTP!" })
                : Json(new { success = false, message = "Không thể gửi mail." });
        }

        // Bước 2: Kiểm tra OTP (Hỗ trợ cả Email hoặc Username)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(string email, string username, string otp)
        {
            // Nếu frontend gửi username thay vì email, ta tự đi tìm email
            if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(username))
            {
                var user = await _loginService.GetUsersByUsername(username);
                if (user != null) email = user.Email;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return Json(new { success = false, message = "Thiếu thông tin xác thực!" });

            var isValid = await _loginService.VerifyOtpAsync(email, otp);

            return isValid
                ? Json(new { success = true, message = "Xác thực thành công!" })
                : Json(new { success = false, message = "Mã OTP không đúng hoặc đã hết hạn." });
        }

        // Bước 3: Đổi mật khẩu mới
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string username, string newPassword)
        {
            // --- 1. VALIDATION MẠNH MẼ (Chặn hacker dùng tool) ---

            // Check độ dài
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
                return Json(new { success = false, message = "Mật khẩu phải dài ít nhất 8 ký tự." });

            // Check độ phức tạp: Phải có Chữ hoa, Số, Ký tự đặc biệt
            // Regex.IsMatch trả về true nếu tìm thấy mẫu
            bool hasUpper = Regex.IsMatch(newPassword, @"[A-Z]");
            bool hasDigit = Regex.IsMatch(newPassword, @"[0-9]");
            bool hasSpecial = Regex.IsMatch(newPassword, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasUpper || !hasDigit || !hasSpecial)
            {
                return Json(new { success = false, message = "Mật khẩu yếu: Cần ít nhất 1 chữ hoa, 1 số và 1 ký tự đặc biệt." });
            }

            // --- 2. XỬ LÝ LOGIC TÌM TÀI KHOẢN ---

            // Nếu frontend chỉ gửi username (do user chọn cách tìm bằng tên), ta tự tìm email
            if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(username))
            {
                var user = await _loginService.GetUsersByUsername(username);
                if (user != null) email = user.Email;
            }

            // Nếu tìm mãi vẫn không ra email -> Lỗi
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Lỗi định danh tài khoản." });

            // --- 3. GỌI SERVICE ĐỔI PASS ---
            await _loginService.ResetPasswordAsync(email, newPassword);

            return Json(new { success = true, message = "Đổi mật khẩu thành công! Hãy đăng nhập ngay." });
        }

        #endregion

        #region 4. ĐĂNG NHẬP GOOGLE (EXTERNAL AUTH)

        [AllowAnonymous]
        public IActionResult LoginByGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
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

            // Logic: Nếu chưa có thì tự tạo user mới
            var user = await _loginService.GetUsersByEmail(email);
            if (user == null)
            {
                user = new Customer
                {
                    Username = email,
                    Email = email,
                    FullName = name,
                    CreatedAt = DateTime.Now,
                    PasswordHash = "GOOGLE_AUTH_NO_PASSWORD",
                    Address = "Chưa cập nhật",
                    Phone = ""
                };
                await _loginService.addAsync(user);
                await _loginService.SaveChangeAsync();
            }

            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region 4. HÀM PHỤ TRỢ (HELPERS)

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

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return email;
            var parts = email.Split('@');
            var local = parts[0];
            if (local.Length <= 2) return email;
            return $"{local.Substring(0, 2)}*****{local.Substring(local.Length - 1, 1)}@{parts[1]}";
        }

        #endregion
    }
}