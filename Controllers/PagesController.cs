using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext; 
using Microsoft.EntityFrameworkCore;

namespace PcSaler.Controllers
{
    public class PagesController : Controller
    {
        private readonly PCShopContext _context; // Thay tên Context cho đúng với project ông

        public PagesController(PCShopContext context)
        {
            _context = context;
        }

        // 1. Trang Khuyến mãi (Lấy 8 sản phẩm ngẫu nhiên làm Deal)
        public async Task<IActionResult> Promotion()
        {
            // Lấy 8 sản phẩm bất kỳ (hoặc lấy theo tiêu chí rẻ/đắt tùy ông)
            // OrderBy(x => Guid.NewGuid()) là cách lấy Random trong SQL
            var deals = await _context.Products
                                      .OrderBy(x => Guid.NewGuid())
                                      .Take(8)
                                      .ToListAsync();

            return View(deals);
        }

        // ... Các Action khác (Contact, Warranty) giữ nguyên
        public IActionResult Contact() { return View(); }
        public IActionResult Warranty() { return View(); }
    }
}