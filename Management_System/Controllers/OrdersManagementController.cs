using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Services;

namespace PcSaler.Management_System.Controllers
{
    [Area("Management")]
    public class OrdersManagementController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CustomerService _customerService;

        public OrdersManagementController(OrderService orderService, CustomerService customerService)
        {
            _orderService = orderService;
            _customerService = customerService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = new SelectList(await _customerService.GetAllCustomersAsync(), "CustomerID", "FullName");
            ViewBag.OrderStatuses = new SelectList(await _orderService.GetAllOrderStatusesAsync(), "StatusID", "StatusName");
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    order.OrderDate = DateTime.Now;
                    await _orderService.CreateOrderAsync(order);
                    TempData["SuccessMessage"] = "Đơn hàng đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo đơn hàng: " + ex.Message;
                }
            }
            ViewBag.Customers = new SelectList(await _customerService.GetAllCustomersAsync(), "CustomerID", "FullName", order.CustomerID);
            ViewBag.OrderStatuses = new SelectList(await _orderService.GetAllOrderStatusesAsync(), "StatusID", "StatusName", order.CurrentStatusID);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.Customers = new SelectList(await _customerService.GetAllCustomersAsync(), "CustomerID", "FullName", order.CustomerID);
            ViewBag.OrderStatuses = new SelectList(await _orderService.GetAllOrderStatusesAsync(), "StatusID", "StatusName", order.CurrentStatusID);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.UpdateOrderAsync(order);
                    TempData["SuccessMessage"] = "Đơn hàng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật đơn hàng: " + ex.Message;
                }
            }
            ViewBag.Customers = new SelectList(await _customerService.GetAllCustomersAsync(), "CustomerID", "FullName", order.CustomerID);
            ViewBag.OrderStatuses = new SelectList(await _orderService.GetAllOrderStatusesAsync(), "StatusID", "StatusName", order.CurrentStatusID);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Đơn hàng đã được xóa thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa đơn hàng!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa đơn hàng: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
