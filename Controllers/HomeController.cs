using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using PcSaler.DBcontext; // [THÊM] Để dùng PCShopContext
using Microsoft.EntityFrameworkCore; // [THÊM] Để dùng FindAsync

namespace PcSaler.Controllers
{
    public class HomeController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;
        private readonly IPcBuildService _pcBuildService;
        private readonly PCShopContext _context; // [THÊM] Context để lấy ảnh

        // Inject thêm PCShopContext vào Constructor
        public HomeController(
            ProductService productService,
            CategoryService categoryService,
            IPcBuildService pcBuildService,
            PCShopContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _pcBuildService = pcBuildService;
            _context = context;
        }

        // --- [MỚI] Action lấy ảnh PC Build từ Database ---
        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            // Tìm PC Build theo ID
            var pcBuild = await _context.PCBuilds.FindAsync(id);

            // Nếu tìm thấy và có ảnh (byte[])
            if (pcBuild != null && pcBuild.PCImage != null && pcBuild.PCImage.Length > 0)
            {
                return File(pcBuild.PCImage, "image/jpeg");
            }

            // Nếu không có ảnh, trả về ảnh mặc định trong thư mục wwwroot
            return Redirect("/images/pc-setup.png");
        }
        // ------------------------------------------------

        public async Task<IActionResult> Index(int? cat, string? q)
        {
            // 1. Lấy danh sách danh mục và sản phẩm thường
            var model = await _categoryService.GetCategoryProducts(cat, q);

            // 2. Lấy danh sách PC Bộ (Dữ liệu này đã được Repository xử lý đường dẫn ảnh)
            var pcBuilds = await _pcBuildService.GetAllPCBuild();

            // 3. LOGIC HỢP NHẤT
            if (pcBuilds != null && pcBuilds.Any())
            {
                var mappedBuilds = pcBuilds.Select(x => new ProductListViewModel
                {
                    ProductID = x.PCBuildID,
                    ProductName = x.PCBuildName,
                    Price = x.TotalPrice,

                    // [SỬA] Ép cứng đường dẫn trỏ về Action GetImage ở trên
                    // Dù Repository có trả về gì thì ở đây ta chốt hạ đường dẫn này cho chắc chắn
                    ImageURL = "/Home/GetImage/" + x.PCBuildID,

                    Stock = 100
                }).ToList();

                // 4. Tìm danh mục đích (PC Build)
                var targetCategory = model.FirstOrDefault(c => c.CategoryID == 15);

                if (targetCategory == null)
                {
                    targetCategory = model.FirstOrDefault(c => c.CategoryName.Trim().ToUpper().Contains("PC BUILD"));
                }

                if (targetCategory != null)
                {
                    targetCategory.Products = mappedBuilds;
                }
                else
                {
                    var virtualCat = new CategoryViewModel
                    {
                        CategoryID = -999,
                        CategoryName = "PC BUILD",
                        ComponentType = "PC_BUILD",
                        Products = mappedBuilds
                    };
                    model.Insert(0, virtualCat);
                }
            }

            ViewBag.Categories = await _categoryService.GetAllCategories();
            ViewBag.SelectedCat = cat;
            ViewBag.Query = q ?? "";

            return View(model);
        }
    }
}