using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PCShopContext _context;

        public PaymentRepository(PCShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(p => p.PaymentID == id);
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderID == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            try
            {
                _context.Entry(payment).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return payment;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(payment.PaymentID))
                {
                    throw new ArgumentException("Payment not found");
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                    return false;

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Handle foreign key constraint violations
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547)
                {
                    throw new InvalidOperationException("Không thể xóa thanh toán này vì nó có dữ liệu liên quan khác.");
                }
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Payments.AnyAsync(p => p.PaymentID == id);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Payments.SumAsync(p => p.Amount);
        }
    }
}
