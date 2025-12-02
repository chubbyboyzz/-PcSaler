using Microsoft.AspNetCore.Mvc;
using PcSaler.Services; // Namespace chứa CartService
using System.Security.Claims;

namespace PcSaler.ViewComponents
{
    public class CartMenuViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartMenuViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Mặc định là 0 (cho khách vãng lai hoặc chưa login)
            int count = 0;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Nếu đã đăng nhập, lấy số lượng từ Database ngay lập tức
                var userIdClaim = ((ClaimsPrincipal)User).FindFirst("id")?.Value
                               ?? ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int customerId))
                {
                    count = await _cartService.GetCartItemCount(customerId);
                }
            }

            // Trả về View với số lượng tính được
            return View(count);
        }
    }
}