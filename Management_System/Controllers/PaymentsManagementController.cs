using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Services;

namespace PcSaler.Management_System.Controllers
{
    [Area("Management")]
    public class PaymentsManagementController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly OrderService _orderService;

        public PaymentsManagementController(PaymentService paymentService, OrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _paymentService.GetPaymentByIdAsync(id.Value);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Orders = new SelectList(await _orderService.GetAllOrdersAsync(), "OrderID", "OrderID");
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    payment.PaymentDate = DateTime.Now;
                    await _paymentService.CreatePaymentAsync(payment);
                    TempData["SuccessMessage"] = "Thanh toán đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo thanh toán: " + ex.Message;
                }
            }
            ViewBag.Orders = new SelectList(await _orderService.GetAllOrdersAsync(), "OrderID", "OrderID", payment.OrderID);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _paymentService.GetPaymentByIdAsync(id.Value);
            if (payment == null)
            {
                return NotFound();
            }
            ViewBag.Orders = new SelectList(await _orderService.GetAllOrdersAsync(), "OrderID", "OrderID", payment.OrderID);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment)
        {
            if (id != payment.PaymentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _paymentService.UpdatePaymentAsync(payment);
                    TempData["SuccessMessage"] = "Thanh toán đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thanh toán: " + ex.Message;
                }
            }
            ViewBag.Orders = new SelectList(await _orderService.GetAllOrdersAsync(), "OrderID", "OrderID", payment.OrderID);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _paymentService.GetPaymentByIdAsync(id.Value);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _paymentService.DeletePaymentAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Thanh toán đã được xóa thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa thanh toán!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thanh toán: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
