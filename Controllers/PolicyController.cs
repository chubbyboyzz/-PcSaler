using Microsoft.AspNetCore.Mvc;

namespace PcSaler.Controllers
{
    public class PolicyController : Controller
    {
        [HttpGet("/chinh-sach-bao-hanh", Name = "Policy_Warranty")]
        public IActionResult Warranty()
        {
            ViewData["Heading"] = "Chính sách bảo hành";
            ViewData["Subheading"] = "Cam kết bảo hành chính hãng, xử lý nhanh – minh bạch.";
            return View();
        }

        [HttpGet("/chinh-sach-van-chuyen", Name = "Policy_Shipping")]
        public IActionResult Shipping()
        {
            ViewData["Heading"] = "Chính sách vận chuyển";
            ViewData["Subheading"] = "Giao hàng toàn quốc, linh hoạt – minh bạch chi phí.";
            return View();
        }

        [HttpGet("/chinh-sach-dieu-khoan", Name = "Policy_General")]
        public IActionResult General()
        {
            ViewData["Heading"] = "Chính sách & Điều khoản";
            ViewData["Subheading"] = "Minh bạch điều kiện mua sắm, thanh toán & bảo mật.";
            return View();
        }
    }
}
