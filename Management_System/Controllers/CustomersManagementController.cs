using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Services;

namespace PcSaler.Management_System.Controllers
{
    [Area("Management")]
    public class CustomersManagementController : Controller
    {
        private readonly CustomerService _customerService;

        public CustomersManagementController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    customer.CreatedAt = DateTime.Now;
                    await _customerService.CreateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Khách hàng đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo khách hàng: " + ex.Message;
                }
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _customerService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Khách hàng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật khách hàng: " + ex.Message;
                }
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Khách hàng đã được xóa thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa khách hàng: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
