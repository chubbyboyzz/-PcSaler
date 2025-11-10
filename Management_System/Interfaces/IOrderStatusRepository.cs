using PcSaler.DBcontext.Entites;

namespace PcSaler.Management_System.Interfaces
{
    public interface IOrderStatusRepository
    {
        Task<IEnumerable<OrderStatus>> GetAllAsync();
        Task<OrderStatus?> GetByIdAsync(int id);
        Task<OrderStatus> CreateAsync(OrderStatus status);
        Task<OrderStatus> UpdateAsync(OrderStatus status);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
