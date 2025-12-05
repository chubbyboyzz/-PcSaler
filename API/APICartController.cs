using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcSaler.Models;
using PcSaler.Models.DTOs;
using PcSaler.Services;
using System.Security.Claims;

namespace PcSaler.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class APICartController : ControllerBase
    {
        private readonly CartService _cartService;

        public APICartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("count")]
        [AllowAnonymous] // Cho phép cả khách xem số lượng (nếu cần xử lý logic guest count sau này)
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                // Nếu chưa đăng nhập thì trả về 0 hoặc xử lý logic khác
                if (User.Identity == null || !User.Identity.IsAuthenticated) return Ok(0);
                return Ok(await _cartService.GetCartItemCount(GetCustomerId()));
            }
            catch { return Ok(0); }
        }

        [HttpGet("my-cart")]
        public async Task<IActionResult> GetMyCart()
        {
            try { return Ok(await _cartService.GetCartItems(GetCustomerId())); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // --- [NEW] API CHO KHÁCH VÃNG LAI ---
        [HttpPost("guest-cart")]
        [AllowAnonymous] // Quan trọng: Khách chưa login vẫn gọi được
        public async Task<IActionResult> GetGuestCart([FromBody] List<CartItemViewModel> guestItems)
        {
            try
            {
                var result = await _cartService.GetGuestCartItems(guestItems);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
            var id = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(id!);
        }
    }
}