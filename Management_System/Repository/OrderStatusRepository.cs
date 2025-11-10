using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Repository
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly PCShopContext _context;

        public OrderStatusRepository(PCShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderStatus>> GetAllAsync()
        {
            return await _context.OrderStatuses.ToListAsync();
        }

        public async Task<OrderStatus?> GetByIdAsync(int id)
        {
            return await _context.OrderStatuses.FindAsync(id);
        }

        public async Task<OrderStatus> CreateAsync(OrderStatus status)
        {
            _context.OrderStatuses.Add(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<OrderStatus> UpdateAsync(OrderStatus status)
        {
            _context.Entry(status).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var status = await _context.OrderStatuses.FindAsync(id);
            if (status == null)
                return false;

            _context.OrderStatuses.Remove(status);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.OrderStatuses.AnyAsync(s => s.StatusID == id);
        }
    }
}
