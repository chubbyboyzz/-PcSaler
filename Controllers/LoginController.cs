using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;

        public LoginController(LoginService loginService, IMemoryCache memoryCache)
        {
            _loginService = loginService;
            _memoryCache = memoryCache;
        }

        #region 1. XỬ LÝ ĐĂNG NHẬP (LOGIN)

        // GET: Login (Chặn truy cập trực tiếp bằng đường dẫn)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string returnUrl = "/")
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            ViewData["ReturnUrl"] = returnUrl;
            // Trả về dòng này để debug nếu Form gửi sai Method
            return Content("Vui lòng đăng nhập thông qua nút Đăng nhập trên trang chủ (Method POST).");
        }

        // POST: Login (Xử lý chính)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model, string returnUrl = "/")
        {
            // 1. Validate dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            // 2. Kiểm tra xem tài khoản có đang bị KHÓA TẠM THỜI không?
            string lockoutKey = $"Lockout_{model.Username}";
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime unlockTime))
            {
                if (DateTime.Now < unlockTime)
                {
                    var remainingSeconds = (int)(unlockTime - DateTime.Now).TotalSeconds;
                    return Json(new
                    {
                        success = false,
                        isLocked = true, // Cờ báo hiệu khóa để JS đếm ngược
                        remainingTime = remainingSeconds,
                        message = $"Tài khoản tạm khóa. Vui lòng thử lại sau {remainingSeconds} giây."
                    });
                }
            }

            // 3. Thử đăng nhập với DB
            var user = await _loginService.LoginUserAsync(model.Username, model.Password);

            if (user == null)
            {
                // --- LOGIC ĐẾM LẦN SAI & KHÓA ---

                // Bước A: Kiểm tra xem User này có TỒN TẠI thật không?
                var existingUser = await _loginService.GetUsersByUsername(model.Username);

                if (existingUser == null)
                {
                    // User không tồn tại -> Báo lỗi thường, KHÔNG ĐẾM, KHÔNG KHÓA
                    return Json(new
                    {
                        success = false,
                        isLocked = false,
                        message = "Tài khoản hoặc mật khẩu không chính xác!"
                    });
                }

                // Bước B: User có thật nhưng sai mật khẩu -> BẮT ĐẦU ĐẾM
                string failCountKey = $"FailCount_{model.Username}";

                // Lấy số lần sai hiện tại (mặc định 0)
                int failCount = _memoryCache.GetOrCreate(failCountKey, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(15); // Reset sau 15p
                    return 0;
                });

                failCount++; // Tăng số lần sai
                _memoryCache.Set(failCountKey, failCount);

                // Bước C: Kiểm tra ngưỡng phạt (Sai từ lần 3 trở đi mới khóa)
                if (failCount >= 3)
                {
                    // Công thức: 30 * 2^(số lần quá hạn). Lần 3 = 30s, Lần 4 = 60s...
                    double waitSeconds = 30 * Math.Pow(2, failCount - 3);

                    var lockUntil = DateTime.Now.AddSeconds(waitSeconds);
                    _memoryCache.Set(lockoutKey, lockUntil, TimeSpan.FromSeconds(waitSeconds));

                    return Json(new
                    {
                        success = false,
                        isLocked = true,
                        remainingTime = (int)waitSeconds,
                        message = $"Sai mật khẩu {failCount} lần. Bị khóa trong {waitSeconds} giây."
                    });
                }
                else
                {
                    // Sai dưới 3 lần -> Chỉ báo lỗi
                    int attemptsLeft = 3 - failCount;
                    return Json(new
                    {
                        success = false,
                        isLocked = false,
                        message = $"Mật khẩu không đúng! (Còn {attemptsLeft} lần thử)"
                    });
                }
            }

            // 4. Đăng nhập thành công -> Kiểm tra Captcha
            string? verifiedToken = HttpContext.Session.GetString("CaptchaVerifiedToken");
            if (string.IsNullOrEmpty(verifiedToken))
            {
                return Json(new { success = false, requireCaptcha = true, message = "Vui lòng xác thực bảo mật!" });
            }

            // Xóa Token Captcha và Xóa án phạt
            HttpContext.Session.Remove("CaptchaVerifiedToken");
            _memoryCache.Remove($"Lockout_{model.Username}");
            _memoryCache.Remove($"FailCount_{model.Username}");

            // Ghi Cookie đăng nhập
            await SignInUser(user);

            // Điều hướng
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl) || returnUrl == "/")
            {
                returnUrl = Url.Action("Index", "Home");
            }

            return Json(new { success = true, redirectUrl = returnUrl });
        }

        #endregion

        #region 2. XỬ LÝ ĐĂNG XUẤT (LOGOUT)

        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa sạch Session (Giỏ hàng, Captcha...)
            HttpContext.Session.Clear();

            // Quay về trang chủ
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region 3. XỬ LÝ QUÊN MẬT KHẨU (FORGOT PASSWORD API)

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserInfo(string username)
        {
            if (string.IsNullOrEmpty(username)) return Json(new { success = false, message = "Vui lòng nhập tên đăng nhập!" });

            var user = await _loginService.GetUsersByUsername(username);

            if (user == null) return Json(new { success = false, message = "Tên đăng nhập không tồn tại." });
            if (string.IsNullOrEmpty(user.Email)) return Json(new { success = false, message = "Tài khoản chưa cập nhật Email." });

            return Json(new
            {
                success = true,
                fullName = user.FullName ?? user.Username,
                maskedEmail = MaskEmail(user.Email)
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Vui lòng nhập Email!" });

            var result = await _loginService.SendForgotPasswordEmailAsync(email);
            return result
                ? Json(new { success = true, message = "Mã OTP đã được gửi." })
                : Json(new { success = false, message = "Email này chưa đăng ký." });
        }

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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(string email, string username, string otp)
        {
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
                : Json(new { success = false, message = "Mã OTP không đúng hoặc hết hạn." });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string username, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
                return Json(new { success = false, message = "Mật khẩu phải dài ít nhất 8 ký tự." });

            bool hasUpper = Regex.IsMatch(newPassword, @"[A-Z]");
            bool hasDigit = Regex.IsMatch(newPassword, @"[0-9]");
            bool hasSpecial = Regex.IsMatch(newPassword, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasUpper || !hasDigit || !hasSpecial)
            {
                return Json(new { success = false, message = "Mật khẩu yếu: Cần ít nhất 1 chữ hoa, 1 số và 1 ký tự đặc biệt." });
            }

            if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(username))
            {
                var user = await _loginService.GetUsersByUsername(username);
                if (user != null) email = user.Email;
            }

            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Lỗi định danh tài khoản." });

            await _loginService.ResetPasswordAsync(email, newPassword);
            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        #endregion

        #region 4. ĐĂNG NHẬP GOOGLE & TỰ ĐỘNG ĐĂNG KÝ (API)

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

            // Nếu hủy hoặc lỗi -> Về trang chủ
            if (!result.Succeeded) return RedirectToAction("Index", "Home");

            // Lấy thông tin từ Google
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Index", "Home");

            // Kiểm tra xem User đã có trong DB chưa?
            var user = await _loginService.GetUsersByEmail(email);

            if (user == null)
            {
                // [LOGIC TỰ ĐỘNG ĐĂNG KÝ] Nếu chưa có -> Tạo mới luôn
                user = new Customer
                {
                    Username = email, // Dùng email làm username
                    Email = email,
                    FullName = name ?? "Google User",
                    CreatedAt = DateTime.Now,
                    PasswordHash = "GOOGLE_AUTH_NO_PASSWORD", // Đánh dấu acc này không dùng pass thường
                    Address = "Chưa cập nhật",
                    Phone = ""
                };

                await _loginService.addAsync(user);
                await _loginService.SaveChangeAsync();
            }

            // Đăng nhập luôn cho User
            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region 5. HÀM PHỤ TRỢ (HELPER)

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