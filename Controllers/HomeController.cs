using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index(int? cat, string? q)
        {
            var model = _productService.GetCategoryProducts(cat, q);
            ViewBag.Categories = _productService.GetAllCategories();
            ViewBag.SelectedCat = cat;
            ViewBag.Query = q ?? "";

            return View(model);
        }

        public IActionResult Details(int id)
        {
            var product = _productService.GetProductDetails(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}
