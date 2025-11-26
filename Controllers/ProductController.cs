using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> Index(int? id)
        {
            var products = await _productService.GetProductsByCategory(id);

            return View(products);
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetails(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}