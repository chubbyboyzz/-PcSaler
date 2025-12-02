using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly PCShopContext _context;

        public CustomersController(ICustomerService customerService, IOrderService orderService, PCShopContext context)
        {
            _customerService = customerService;
            _orderService = orderService;
            _context = context;
        }

        // 1. Trang Hồ sơ (Profile)
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // SỬA: Chỉ lấy ID nội bộ Database
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // SỬA: Chuyển hướng về đúng Login/Index nếu chưa có ID
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Index", "Login");

            int userId = int.Parse(userIdClaim); // Parse ngon lành

            var profile = await _customerService.GetProfileByIdAsync(userId);
            if (profile == null) return NotFound();

            // Lấy danh sách đơn hàng
            profile.Orders = await _orderService.GetOrdersByCustomerIdAsync(userId);

            return View(profile);
        }

        // 2. API Cập nhật thông tin 
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileViewModel model)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = string.Join(", ", errors) });
            }

            var customer = await _context.Customers.FindAsync(userId);

            if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

            customer.FullName = model.FullName;
            customer.Phone = model.Phone;
            customer.Address = model.Address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 3. API Lấy chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Include(o => o.CurrentStatus)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.CustomerID == userId);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            var finalAddress = order.ShippingAddress ?? order.Customer.Address;

            var result = new
            {
                orderId = order.OrderID,
                orderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                totalAmount = order.TotalAmount,
                status = order.CurrentStatus?.StatusName ?? "Đang xử lý",
                customerName = order.Customer.FullName,
                shippingAddress = finalAddress,
                items = order.OrderDetails.Select(od => new
                {
                    productName = od.Product.ProductName,
                    image = od.Product.ImageURL,
                    price = od.UnitPrice,
                    quantity = od.Quantity,
                    total = od.UnitPrice * od.Quantity
                }).ToList()
            };

            return Ok(result);
        }
    }
}