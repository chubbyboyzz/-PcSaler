using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Services;

namespace PcSaler.Management_System.Controllers
{
    [Area("Management")]
    public class PCBuildsManagementController : Controller
    {
        private readonly PCBuildManagementService _pcBuildService;

        public PCBuildsManagementController(PCBuildManagementService pcBuildService)
        {
            _pcBuildService = pcBuildService;
        }

        // GET: PCBuilds
        public async Task<IActionResult> Index()
        {
            var pcBuilds = await _pcBuildService.GetAllPCBuildsAsync();
            return View(pcBuilds);
        }

        // GET: PCBuilds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcBuild = await _pcBuildService.GetPCBuildByIdAsync(id.Value);
            if (pcBuild == null)
            {
                return NotFound();
            }

            return View(pcBuild);
        }

        // GET: PCBuilds/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PCBuilds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PCBuild pcBuild)
        {
            if (ModelState.IsValid)
            {
                pcBuild.CreatedAt = DateTime.Now;
                pcBuild.UpdatedAt = DateTime.Now;
                await _pcBuildService.CreatePCBuildAsync(pcBuild);
                TempData["SuccessMessage"] = "Bộ PC đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(pcBuild);
        }

        // GET: PCBuilds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcBuild = await _pcBuildService.GetPCBuildByIdAsync(id.Value);
            if (pcBuild == null)
            {
                return NotFound();
            }
            return View(pcBuild);
        }

        // POST: PCBuilds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PCBuild pcBuild)
        {
            if (id != pcBuild.PCBuildID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                pcBuild.UpdatedAt = DateTime.Now;
                await _pcBuildService.UpdatePCBuildAsync(pcBuild);
                TempData["SuccessMessage"] = "Bộ PC đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(pcBuild);
        }

        // GET: PCBuilds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcBuild = await _pcBuildService.GetPCBuildByIdAsync(id.Value);
            if (pcBuild == null)
            {
                return NotFound();
            }

            return View(pcBuild);
        }

        // POST: PCBuilds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _pcBuildService.DeletePCBuildAsync(id);
            TempData["SuccessMessage"] = "Bộ PC đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
