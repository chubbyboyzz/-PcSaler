using Microsoft.AspNetCore.Mvc;
using PcSaler.Services;
using PcSaler.Models; // Chứa ViewModel
using System.Security.Claims; // Để lấy User ID

namespace PcSaler.Controllers
{
    public class CustomersPCController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly CustomerPCService _customerPCService;

        public CustomersPCController(CategoryService categoryService, CustomerPCService customerPCService)
        {
            _categoryService = categoryService;
            _customerPCService = customerPCService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            // 1. Lấy danh mục (Giữ nguyên)
            var data = await _categoryService.GetCategoryProducts(null, null);
            var categories = data
                .Where(c => c.IsRequiredForBuild == true)
                .Select(c => new
                {
                    id = c.ComponentType,
                    name = c.CategoryName
                }).ToList();
            ViewBag.Categories = categories;

            // 2. XỬ LÝ USER SLOTS AN TOÀN
            // Khởi tạo danh sách rỗng mặc định (để ViewBag không bị null)
            var userSlots = new List<PcSaler.Models.PCBuildDetailViewModel>();

            // Chỉ thực hiện logic lấy Slot khi người dùng ĐÃ ĐĂNG NHẬP
            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Dùng TryParse cho an toàn (tránh lỗi format)
                if (int.TryParse(userIdString, out int userId))
                {
                    // Gọi Service để lấy hoặc tạo 3 slot
                    var slots = await _customerPCService.GetUserSlots(userId);

                    // Nếu service trả về null (lỗi gì đó) thì vẫn giữ là list rỗng
                    if (slots != null)
                    {
                        userSlots = slots;
                    }
                }
            }

            // Gán vào ViewBag (Lúc này userSlots luôn là List, có thể có phần tử hoặc rỗng, nhưng KHÔNG NULL)
            ViewBag.UserSlots = userSlots;

            return View();
        }
    }
}