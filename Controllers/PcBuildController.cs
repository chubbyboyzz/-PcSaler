using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Models;

namespace PcSaler.Controllers
{
    public class PcBuildController : Controller
    {
        // GET: /PcBuildView
        public IActionResult Index()
        {
            // Trả view Razor Build PC
            return View();
        }
    }
    

}
