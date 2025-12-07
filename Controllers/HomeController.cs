using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace PcSaler.Controllers
{
    public class HomeController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;
        // Inject thêm Service xử lý PC Build
        private readonly IPcBuildService _pcBuildService;

        public HomeController(ProductService productService, CategoryService categoryService, IPcBuildService pcBuildService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _pcBuildService = pcBuildService;
        }

        public async Task<IActionResult> Index(int? cat, string? q)
        {
            // 1. Lấy danh sách danh mục và sản phẩm thường từ bảng Products
            // (Lúc này cái danh mục ID 15 "PC build" sẽ không có sản phẩm nào)
            var model = await _categoryService.GetCategoryProducts(cat, q);

            // 2. Lấy danh sách PC Bộ từ bảng PCBuilds (Dữ liệu thật)
            var pcBuilds = await _pcBuildService.GetAllPCBuild();

            // 3. LOGIC HỢP NHẤT (MAPPING)
            if (pcBuilds != null && pcBuilds.Any())
            {
                // Chuyển đổi dữ liệu từ PCBuildDetailViewModel sang ProductListViewModel
                // Để View Index.cshtml có thể hiển thị được
                var mappedBuilds = pcBuilds.Select(x => new ProductListViewModel
                {
                    ProductID = x.PCBuildID,       // Mượn trường ProductID để lưu ID máy bộ
                    ProductName = x.PCBuildName,   // Tên máy bộ
                    Price = x.TotalPrice,          // Giá tổng

                    // Xử lý ảnh: Nếu null thì dùng ảnh mặc định
                    ImageURL = !string.IsNullOrEmpty(x.ImageURL) ? x.ImageURL : "pc-setup.png",

                    // QUAN TRỌNG: Fake số lượng tồn kho để không bị hiện badge "Hết hàng"
                    Stock = 100
                }).ToList();

                // 4. Tìm danh mục đích để bơm dữ liệu vào
                // Ưu tiên 1: Tìm theo ID 15 (Theo ảnh Database ông gửi)
                var targetCategory = model.FirstOrDefault(c => c.CategoryID == 15);

                // Ưu tiên 2: Nếu không thấy ID 15 thì tìm theo tên
                if (targetCategory == null)
                {
                    targetCategory = model.FirstOrDefault(c => c.CategoryName.Trim().ToUpper().Contains("PC BUILD"));
                }

                if (targetCategory != null)
                {
                    // === TRƯỜNG HỢP A: Tìm thấy danh mục trong DB ===
                    // Gán danh sách máy bộ vào danh mục này
                    targetCategory.Products = mappedBuilds;
                }
                else
                {
                    // === TRƯỜNG HỢP B: Không tìm thấy (Dự phòng) ===
                    // Tạo một danh mục ảo và chèn lên đầu trang
                    var virtualCat = new CategoryViewModel
                    {
                        CategoryID = -999, // ID âm để đánh dấu
                        CategoryName = "PC BUILD",
                        ComponentType = "PC_BUILD",
                        Products = mappedBuilds
                    };
                    model.Insert(0, virtualCat);
                }
            }

            // Đổ dữ liệu bổ trợ cho View
            ViewBag.Categories = await _categoryService.GetAllCategories();
            ViewBag.SelectedCat = cat;
            ViewBag.Query = q ?? "";

            return View(model);
        }
    }
}