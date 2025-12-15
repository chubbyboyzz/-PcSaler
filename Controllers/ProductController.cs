using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    public class ProductController : Controller
    {
        // Khai báo Dependency Injection (Chỉ 1 lần duy nhất)
        private readonly ProductService _productService;
        private readonly PCShopContext _context;

        // Constructor: Tiêm Service và DBContext vào Controller
        public ProductController(ProductService productService, PCShopContext context)
        {
            _productService = productService; // Dùng cho logic nghiệp vụ (Index, Details)
            _context = context;               // Dùng để truy xuất trực tiếp (GetImage, Accessories)
        }

        // 1. TRANG DANH SÁCH SẢN PHẨM
        public async Task<IActionResult> Index(int? id)
        {
            // Kiểm tra nếu là danh mục PC Build thì redirect sang Controller chuyên biệt
            if (id.HasValue)
            {
                string? type = await _productService.GetCategoryType(id.Value);
                if (type == "PC")
                {
                    return RedirectToAction("Index", "PcBuild");
                }
            }

            // Lấy danh sách sản phẩm theo danh mục
            var products = await _productService.GetProductsByCategory(id);
            return View(products);
        }

        // 2. TRANG CHI TIẾT SẢN PHẨM
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetails(id);

            // Nếu không tìm thấy sản phẩm thì trả về trang lỗi 404
            if (product == null) return NotFound();

            return View(product);
        }

        // 3. API GỢI Ý TÌM KIẾM (Dùng cho Ajax ở thanh Search)
        [HttpGet]
        [Route("api/product/search-suggestions")]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            var suggestions = await _productService.GetProductQuery(query);
            return Ok(suggestions);
        }

        // 4. TRANG PHỤ KIỆN (ACCESSORIES)
        public async Task<IActionResult> Accessories(string type = "ALL")
        {
            // Danh sách các mã ComponentType thuộc nhóm phụ kiện
            var accessoryTypes = new List<string> { "MOUSE", "KEYBOARD", "HEADSET", "MONITOR", "CHAIR" };

            // Query trực tiếp từ Context để filter nhanh
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (type == "ALL")
            {
                // Lấy tất cả phụ kiện
                query = query.Where(p => accessoryTypes.Contains(p.Category.ComponentType));
            }
            else
            {
                // Lọc theo loại cụ thể (ví dụ: chỉ lấy MOUSE)
                query = query.Where(p => p.Category.ComponentType == type);
            }

            var products = await query.OrderByDescending(p => p.ProductID).ToListAsync();

            // ViewBag để giữ trạng thái active cho menu bên View
            ViewBag.CurrentType = type;

            return View(products);
        }

        // ============================================================
        // 5. HÀM QUAN TRỌNG: LẤY ẢNH TỪ DATABASE (BINARY DATA)
        // ============================================================
        // Route này sẽ được gọi từ thẻ img: src="/Product/GetImage/123"
        [HttpGet]
        [Route("Product/GetImage/{id}")]
        public async Task<IActionResult> GetImage(int id)
        {
            // Tìm sản phẩm theo ID
            var product = await _context.Products.FindAsync(id);

            // Kiểm tra: 
            // 1. Sản phẩm có tồn tại?
            // 2. Cột ProductImage có dữ liệu không?
            // 3. Độ dài dữ liệu > 0?
            if (product != null && product.ProductImage != null && product.ProductImage.Length > 0)
            {
                // Trả về file ảnh dạng JPEG (Trình duyệt sẽ tự render)
                return File(product.ProductImage, "image/jpeg");
            }

            // Fallback: Nếu không có ảnh trong DB, trả về ảnh mặc định trong thư mục wwwroot
            // Đảm bảo ông có file no-image.png trong thư mục wwwroot/images/ nhé
            return Redirect("/images/no-image.png");
        }
    }
}