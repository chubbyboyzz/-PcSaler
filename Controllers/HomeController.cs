using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Services;
using System.Threading.Tasks;

namespace PcSaler.Controllers
{
    public class HomeController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;

        public HomeController(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? cat, string? q)
        {
            var model = await _categoryService.GetCategoryProducts(cat, q);
            ViewBag.Categories = await _categoryService.GetAllCategories();
            ViewBag.SelectedCat = cat;
            ViewBag.Query = q ?? "";

            return View(model);
        }
    }
}
