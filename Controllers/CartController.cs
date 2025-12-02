using Microsoft.AspNetCore.Mvc;
using PcSaler.Models;
using PcSaler.Services;

namespace PcSaler.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public IActionResult Index()
        {
            return View();
        }
    }
}
