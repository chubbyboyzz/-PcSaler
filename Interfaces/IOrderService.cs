using PcSaler.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PcSaler.Interfaces
{
    public interface IOrderService
    {
        // Lấy danh sách đơn hàng của một khách
        Task<List<OrderHistoryViewModel>> GetOrdersByCustomerIdAsync(int customerId);
    }
}