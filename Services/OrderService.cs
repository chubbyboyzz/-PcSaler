using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites; // Thêm dòng này để dùng Order, OrderDetail
using PcSaler.Interfaces;
using PcSaler.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcSaler.Services
{
    public class OrderService : IOrderService
    {
        private readonly PCShopContext _context;
        private readonly CartService _cartService; // [NEW] Cần cái này để lấy giỏ hàng khi đặt hàng

        // [UPDATE] Inject thêm CartService vào Constructor
        public OrderService(PCShopContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // ==========================================
        // PHẦN 1: LOGIC ĐẶT HÀNG (MỚI THÊM VÀO)
        // ==========================================
        public async Task<bool> PlaceOrder(int customerId, CheckoutViewModel model)
        {
            // Dùng Transaction để đảm bảo tính toàn vẹn (Lỗi là rollback hết)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lấy dữ liệu giỏ hàng hiện tại
                var cartItems = await _cartService.GetCartItems(customerId);
                if (cartItems == null || !cartItems.Any()) return false;

                // 2. Lấy Status mặc định (Pending)
                var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Pending")
                                    ?? await _context.OrderStatuses.FirstOrDefaultAsync(); // Fallback nếu chưa seed data

                if (pendingStatus == null) throw new Exception("Chưa có dữ liệu OrderStatus trong DB!");

                // 3. Tạo Order Header
                var order = new Order
                {
                    CustomerID = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(x => x.Price * x.Quantity),
                    CurrentStatusID = pendingStatus.StatusID
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để sinh OrderID

                // 4. Tạo Order Details (Copy từ Cart sang OrderDetail)
                var orderDetails = new List<OrderDetail>();
                foreach (var item in cartItems)
                {
                    orderDetails.Add(new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ItemID,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    });
                }
                _context.OrderDetails.AddRange(orderDetails);

                // 5. Tạo Payment Record (Ghi nhận phương thức thanh toán & Địa chỉ)
                var payment = new Payment
                {
                    OrderID = order.OrderID,
                    Amount = order.TotalAmount,
                    PaymentMethod = model.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Note = $"Giao đến: {model.Address}, SĐT: {model.Phone}, Email: {model.Email}. Ghi chú: {model.Note}"
                };
                _context.Payments.Add(payment);

                // 6. Xóa Giỏ hàng (Clear Cart sau khi mua xong)
                var cart = await _context.Carts.Include(c => c.CartItems)
                                         .FirstOrDefaultAsync(c => c.CustomerID == customerId);
                if (cart != null && cart.CartItems != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Xác nhận mọi thứ thành công
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Có lỗi thì hoàn tác
                // Nên log lỗi ra đây (Console.WriteLine(ex.Message))
                return false;
            }
        }

        // ==========================================
        // PHẦN 2: LOGIC LỊCH SỬ ĐƠN HÀNG (CŨ)
        // ==========================================
        public async Task<List<OrderHistoryViewModel>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _context.Orders
                .Include(o => o.CurrentStatus)
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    StatusName = o.CurrentStatus.StatusName,
                    StatusColorClass = GetStatusColor(o.CurrentStatus.StatusName)
                })
                .ToListAsync();

            return orders;
        }

        private static string GetStatusColor(string statusName)
        {
            return statusName switch
            {
                "Pending" => "bg-warning text-dark",
                "Processing" => "bg-info text-dark",
                "Shipped" => "bg-primary",
                "Delivered" => "bg-success",
                "Cancelled" => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}