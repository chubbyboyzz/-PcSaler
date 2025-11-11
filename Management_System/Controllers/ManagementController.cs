using Microsoft.AspNetCore.Mvc;

namespace PcSaler.Management_System.Controllers
{
    [Area("Management")]
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
