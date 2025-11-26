using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.Models;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    [Authorize]
    [Route("api/[controller]")] // [controller] sẽ lấy tên class bỏ chữ "Controller" -> "APICart"
    [ApiController]
    public class APICartController : ControllerBase // <--- SỬA DÒNG NÀY (Cũ là CartController)
    {
        private readonly CartService _cartService;

        public APICartController(CartService cartService)
        {
            _cartService = cartService;
        }

        // --- CÁC API GIỮ NGUYÊN ---

        [HttpGet("count")]
        public async Task<IActionResult> GetCartCount()
        {
            try { return Ok(await _cartService.GetCartItemCount(GetCustomerId())); }
            catch { return Ok(0); }
        }

        [HttpGet("my-cart")]
        public async Task<IActionResult> GetMyCart()
        {
            try { return Ok(await _cartService.GetCartItems(GetCustomerId())); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemViewModel dto)
        {
            try
            {
                await _cartService.AddToCart(GetCustomerId(), dto);
                return Ok(new { message = "Thêm thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartDto dto)
        {
            var result = await _cartService.UpdateQuantity(GetCustomerId(), dto.ItemID, dto.ItemType, dto.Quantity);
            return result ? Ok() : BadRequest(new { message = "Lỗi cập nhật" });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveItem([FromBody] RemoveCartDto dto)
        {
            var result = await _cartService.RemoveItem(GetCustomerId(), dto.ItemID, dto.ItemType);
            return result ? Ok() : BadRequest(new { message = "Lỗi xóa" });
        }

        private int GetCustomerId()
        {
            var id = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(id!);
        }
    }

    public class UpdateCartDto { public int ItemID { get; set; } public string ItemType { get; set; } public int Quantity { get; set; } }
    public class RemoveCartDto { public int ItemID { get; set; } public string ItemType { get; set; } }
}