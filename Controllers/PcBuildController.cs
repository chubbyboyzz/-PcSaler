using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Models;

namespace PcSaler.Controllers
{
    public class PcBuildController : Controller
    {
        private readonly PCShopContext _context;

        public PcBuildController(PCShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var categories = _context.Categories
                // 1. Sửa "ParentCategoryID == 1" thành "IsRequiredForBuild == true"
                .Where(c => c.IsRequiredForBuild == true)
                .Select(c => new {
                    id = c.ComponentType,
                    // 2. Sửa "CategoryName" thành "Description" để lấy tên đúng
                    name = c.Description
                })
                .ToList();

            ViewBag.Categories = categories;
            return View();
        }
    }

}
