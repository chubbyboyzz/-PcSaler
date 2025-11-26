using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
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

        public OrderService(PCShopContext context)
        {
            _context = context;
        }

        public async Task<List<OrderHistoryViewModel>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _context.Orders
                .Include(o => o.CurrentStatus) // Nhớ Include bảng trạng thái
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.OrderDate) // Đơn mới nhất lên đầu
                .Select(o => new OrderHistoryViewModel
                {
                    OrderID = o.OrderID,
                    //OrderDate = o.OrderDate ?? DateTime.MinValue,
                    //TotalAmount = o.TotalAmount ?? 0,
                    StatusName = o.CurrentStatus.StatusName,
                    // Logic tô màu trạng thái đơn giản (Hardcode tạm, sau này nên làm bảng riêng)
                    StatusColorClass = GetStatusColor(o.CurrentStatus.StatusName)
                })
                .ToListAsync();

            return orders;
        }

        // Hàm phụ trợ để chọn màu badge cho đẹp
        private static string GetStatusColor(string statusName)
        {
            return statusName switch
            {
                "Pending" => "bg-warning text-dark",   // Chờ xử lý: Vàng
                "Processing" => "bg-info text-dark",   // Đang xử lý: Xanh dương nhạt
                "Shipped" => "bg-primary",             // Đã giao đi: Xanh dương đậm
                "Delivered" => "bg-success",           // Giao thành công: Xanh lá
                "Cancelled" => "bg-danger",            // Hủy: Đỏ
                _ => "bg-secondary"                    // Mặc định: Xám
            };
        }
    }
}