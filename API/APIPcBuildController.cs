using Microsoft.AspNetCore.Mvc;
using PcSaler.Models;
using PcSaler.Services; // Đảm bảo namespace này đúng
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.WebSockets; // Để dùng .Contains, .Keys

namespace PcSaler.Controllers
{
    [ApiController]
    [Route("api/pcbuild")] // Route gốc
    public class APIPcBuildController : ControllerBase
    {
        private readonly CustomerPCService _customerPCService;
        private readonly ProductService _productService; // Thêm Service tìm kiếm

        // Inject cả 2 Service vào Constructor
        public APIPcBuildController(CustomerPCService customerPCService, ProductService productService)
        {
            _customerPCService = customerPCService;
            _productService = productService;
        }

        // ==========================================
        // PHẦN 1: TÌM KIẾM SẢN PHẨM (Chuyển từ ProductsController sang)
        // Endpoint: GET /api/pcbuild/search
        // ==========================================
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts()
        {
            var query = HttpContext.Request.Query;

            // SỬA: Đảm bảo tên class Model đúng (ProductSearchInputModel)
            var input = new ProductSearchInputViewModel
            {
                Category = query["category"],
                Search = query["search"].ToString().ToLower(),
                Page = int.TryParse(query["page"], out int p) ? p : 1,
                PageSize = int.TryParse(query["pageSize"], out int ps) ? ps : 10,
                MinPrice = decimal.TryParse(query["minPrice"], out decimal min) ? min : 0,
                MaxPrice = decimal.TryParse(query["maxPrice"], out decimal max) ? max : decimal.MaxValue
            };

            var knownKeys = new[] { "category", "search", "page", "pageSize", "minPrice", "maxPrice", "_" };
            foreach (var key in query.Keys)
            {
                if (!knownKeys.Contains(key))
                {
                    string value = query[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        input.DynamicAttributes.Add(key, value);
                    }
                }
            }

            // Gọi hàm Search (Lưu ý: ProductService cần có method Search/SearchProductsAsync nhận input này)
            var result = await _productService.SearchProductsAsync(input);
            return Ok(result);
        }

        // ... Các hàm UpdateSlotItem, RemoveSlotItem, AddToCart giữ nguyên ...
        [HttpPost("update-item")]
        public async Task<IActionResult> UpdateSlotItem([FromForm] int slotId, [FromForm] string type, [FromForm] int productId)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                await _customerPCService.UpdateSlotItem(userId, slotId, type, productId);
                return Ok(new { success = true, message = "Đã lưu" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("remove-item")]
        public async Task<IActionResult> RemoveSlotItem([FromForm] int slotId, [FromForm] string type)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _customerPCService.RemoveSlotItem(userId, slotId, type);
            return Ok(new { success = true });
        }

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromForm] int slotId)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _customerPCService.AddToCartFromSlot(userId, slotId);
                return Ok(new { success = true, message = "Đã thêm vào giỏ!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpGet("my-slots")]
        public async Task<IActionResult> GetMySlots()
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Gọi Service lấy danh sách 3 slot mới nhất
            var slots = await _customerPCService.GetUserSlots(userId);

            return Ok(slots);
        }
    }
}
