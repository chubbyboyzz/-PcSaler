using Microsoft.AspNetCore.Mvc;

namespace PcSaler.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
