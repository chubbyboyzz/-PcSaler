using Microsoft.AspNetCore.Mvc;
using PcSaler.Interfaces;

namespace PcSaler.Controllers
{
    public class PcBuildController : Controller
    {
        // Inject Service (Interface)
        private readonly IPcBuildService _service;

        public PcBuildController(IPcBuildService service)
        {
            _service = service;
        }

        // Trang danh sách PC Bộ (Pre-built)
        public async Task<IActionResult> Index()
        {
            var builds = await _service.GetAllPCBuild();
            return View(builds);
        }

        // Trang chi tiết 1 bộ PC
        public async Task<IActionResult> Details(int id)
        {
            var detail = await _service.GetPCBuildDetails(id);

            if (detail == null) return NotFound();

            return View(detail);
        }
    }
}