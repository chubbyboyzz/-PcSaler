using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PcSaler.Controllers
{
    // Bắt buộc phải đăng nhập mới vào được controller này
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        // Inject 2 Service đã tạo vào
        public CustomersController(ICustomerService customerService, IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
        }

        // Action hiển thị trang Hồ sơ
        public async Task<IActionResult> Profile()
        {
            // 1. Lấy CustomerID từ Cookie đăng nhập (ClaimTypes.NameIdentifier)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                // Nếu không tìm thấy ID (lỗi hy hữu), đá về trang chủ hoặc đăng nhập lại
                return RedirectToAction("Index", "Home");
            }

            // 2. Gọi Service lấy thông tin khách hàng
            var customerInfo = await _customerService.GetProfileByIdAsync(customerId);
            if (customerInfo == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            // 3. Gọi Service lấy lịch sử đơn hàng
            var orderHistory = await _orderService.GetOrdersByCustomerIdAsync(customerId);

            // 4. Đóng gói vào ViewModel tổng
            var viewModel = new ProfilePageViewModel
            {
                CustomerInfo = customerInfo,
                OrderHistory = orderHistory
            };

            // 5. Trả về View
            return View(viewModel);
        }
    }
}