using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // Chuẩn hóa từ khóa về chữ thường để tìm kiếm
            var term = query.ToLower();

            var suggestions = await _productService.GetProductQuery(query);
            return Ok(suggestions);
        }
    }
}