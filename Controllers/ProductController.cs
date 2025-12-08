using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    public class ProductController : Controller
    {
        // Khai báo 2 biến readonly để hứng dữ liệu từ Constructor
        private readonly ProductService _productService;
        private readonly PCShopContext _context;

        // --- CONSTRUCTOR (QUAN TRỌNG NHẤT) ---
        // Sửa: Tiêm cả productService và context vào đây
        public ProductController(ProductService productService, PCShopContext context)
        {
            _productService = productService; // Dùng cho Index, Details
            _context = context;               // Dùng cho Accessories
        }

        // --- CÁC HÀM CŨ (GIỮ NGUYÊN LOGIC) ---
        public async Task<IActionResult> Index(int? id)
        {
            if (id.HasValue)
            {
                string? type = await _productService.GetCategoryType(id.Value);
                if (type == "PC")
                {
                    return RedirectToAction("Index", "PcBuild");
                }
            }
            var products = await _productService.GetProductsByCategory(id);
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetails(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet]
        [Route("api/product/search-suggestions")]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            var suggestions = await _productService.GetProductQuery(query);
            return Ok(suggestions);
        }

        // --- HÀM MỚI (ACCESSORIES) ---
        public async Task<IActionResult> Accessories(string type = "ALL")
        {
            // Danh sách các loại phụ kiện
            var accessoryTypes = new List<string> { "MOUSE", "KEYBOARD", "HEADSET", "MONITOR", "CHAIR" };

            // Bây giờ _context đã có dữ liệu (nhờ Constructor), dùng thoải mái không lo null
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (type == "ALL")
            {
                // Lọc theo danh sách phụ kiện
                query = query.Where(p => accessoryTypes.Contains(p.Category.ComponentType));
            }
            else
            {
                // Lọc theo từng loại cụ thể
                query = query.Where(p => p.Category.ComponentType == type);
            }

            // Sắp xếp và lấy dữ liệu
            var products = await query.OrderByDescending(p => p.ProductID).ToListAsync();

            // Truyền lại type để View hiển thị active menu
            ViewBag.CurrentType = type;

            return View(products);
        }
    }
}