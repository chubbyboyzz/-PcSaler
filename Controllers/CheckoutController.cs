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
            var currentCart = await _cartService.GetCartItems(userId);

            if (currentCart == null || !currentCart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Gán lại dữ liệu để xử lý logic
            model.CartItems = currentCart;
            model.TotalAmount = currentCart.Sum(x => x.Price * x.Quantity);

            // =========================================================
            // [FIX] XÓA BỎ CHECK LỖI CHO CÁC TRƯỜNG KHÔNG CẦN THIẾT
            // =========================================================

            // 1. TotalAmount (Vì ta tự tính ở server, không cần form gửi lên)
            ModelState.Remove("TotalAmount"); // <--- THÊM DÒNG NÀY VÀO

            // 2. CartItems (Nếu model báo lỗi danh sách null)
            ModelState.Remove("CartItems");   // <--- THÊM CẢ DÒNG NÀY CHO CHẮC

            // 3. Xóa lỗi validate thanh toán như cũ
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

            // Giờ mới check IsValid
            if (!ModelState.IsValid)
            {
                // Debug: Đặt breakpoint ở đây xem nó còn lỗi gì trong ModelState.Values
                return View("Index", model);
            }

            var result = await _orderService.PlaceOrder(userId, model);

            if (result)
            {
                return RedirectToAction("Success", new { method = model.PaymentMethod, amount = model.TotalAmount });
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý đơn hàng.");
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