// File: ViewComponents/CategoryMenuViewComponent.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext; // Đảm bảo đúng namespace
using System.Linq;
using System.Threading.Tasks;

namespace PcSaler.ViewComponents // Thay đổi namespace nếu cần
{
    // Class phải kế thừa từ ViewComponent
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly PCShopContext _context;

        public CategoryMenuViewComponent(PCShopContext context)
        {
            _context = context;
        }

        // Phương thức InvokeAsync chạy khi View Component được gọi
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy danh mục menu từ DB (Lấy các trường cần thiết)
            var categories = await _context.Categories
                .Where(c => c.ParentCategoryID == 1) // Hoặc logic lọc menu của bạn
                .Select(c => new
                {
                    CategoryID = c.CategoryID,
                    ComponentType = c.ComponentType,
                    Description = c.Description
                })
                .ToListAsync();

            // Trả về Partial View (Default.cshtml) với dữ liệu này
            return View(categories);
        }
    }
}