using PcSaler.DBcontext.Entites;

namespace PcSaler.Management_System.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<decimal> GetTotalRevenueAsync();
    }
}
