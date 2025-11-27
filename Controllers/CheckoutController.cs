using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly CartService _cartService;
        private readonly IOrderService _orderService;

        public CheckoutController(ICustomerService customerService, CartService cartService, IOrderService orderService)
        {
            _customerService = customerService;
            _cartService = cartService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var userProfile = await _customerService.GetProfileByIdAsync(userId);
            var cartItems = await _cartService.GetCartItems(userId);

            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                FullName = userProfile?.FullName ?? "",
                Email = userProfile?.Email ?? "",
                Phone = userProfile?.Phone ?? "",
                Address = userProfile?.Address ?? "",
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(x => x.Price * x.Quantity),
                PaymentMethod = "COD" // Mặc định
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = GetUserId();

            // =========================================================
            // [FIX QUAN TRỌNG]: TÍNH LẠI TIỀN TỪ SERVER (Tránh lỗi 0đ)
            // =========================================================

            // 1. Lấy lại giỏ hàng từ Database (dữ liệu chuẩn nhất)
            var currentCart = await _cartService.GetCartItems(userId);

            // Nếu giỏ hàng trống thì đá về trang giỏ
            if (currentCart == null || !currentCart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 2. Gán lại dữ liệu vào model để xử lý
            model.CartItems = currentCart;
            // Tính tổng tiền tại đây -> Đảm bảo số tiền luôn đúng
            model.TotalAmount = currentCart.Sum(x => x.Price * x.Quantity);

            // 3. (Tùy chọn) Xóa lỗi validate của các trường không cần thiết
            // Ví dụ: Chọn COD thì không cần bắt nhập số thẻ Visa
            if (model.PaymentMethod != "VISA")
            {
                ModelState.Remove("CardNumber");
                ModelState.Remove("CardHolderName");
                ModelState.Remove("CardExpiry");
                ModelState.Remove("CardCVV");
            }
            if (model.PaymentMethod != "PAYPAL")
            {
                ModelState.Remove("PaypalEmail");
            }

            // Check lại xem Form có hợp lệ không
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Gọi Service tạo đơn hàng
            var result = await _orderService.PlaceOrder(userId, model);

            if (result)
            {
                // Thành công -> Chuyển sang trang Success
                // Truyền method và amount (số tiền vừa tính được) sang để hiển thị
                return RedirectToAction("Success", new { method = model.PaymentMethod, amount = model.TotalAmount });
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.");
                return View("Index", model);
            }
        }

        public IActionResult Success(string? method, decimal? amount)
        {
            ViewBag.PaymentMethod = method;
            ViewBag.Amount = amount;
            return View();
        }

        private int GetUserId()
        {
            var id = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(id!);
        }
    }
}