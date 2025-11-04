using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext;

namespace PcSaler.Controllers
{
    public class CustomersController : Controller
    {
        private readonly PCShopContext? _context;

        public CustomersController(PCShopContext context)
        {
            _context = context;
        }

        // GET: /Customers
        public IActionResult Index()
        {
            var customers = _context?.Customers.ToList();
            return View(customers);
        }
    }
}
