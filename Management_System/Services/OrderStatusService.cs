using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Services
{
    public class OrderStatusService
    {
        private readonly IOrderStatusRepository _orderStatusRepository;

        public OrderStatusService(IOrderStatusRepository orderStatusRepository)
        {
            _orderStatusRepository = orderStatusRepository;
        }

        public async Task<IEnumerable<OrderStatus>> GetAllOrderStatusesAsync()
        {
            return await _orderStatusRepository.GetAllAsync();
        }

        public async Task<OrderStatus?> GetOrderStatusByIdAsync(int id)
        {
            return await _orderStatusRepository.GetByIdAsync(id);
        }

        public async Task<OrderStatus> CreateOrderStatusAsync(OrderStatus status)
        {
            return await _orderStatusRepository.CreateAsync(status);
        }

        public async Task<OrderStatus> UpdateOrderStatusAsync(OrderStatus status)
        {
            return await _orderStatusRepository.UpdateAsync(status);
        }

        public async Task<bool> DeleteOrderStatusAsync(int id)
        {
            return await _orderStatusRepository.DeleteAsync(id);
        }

        public async Task<bool> OrderStatusExistsAsync(int id)
        {
            return await _orderStatusRepository.ExistsAsync(id);
        }
    }
}
