using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Repository
{
    public class PCBuildRepository : IPCBuildRepository
    {
        private readonly PCShopContext _context;

        public PCBuildRepository(PCShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PCBuild>> GetAllAsync()
        {
            return await _context.PCBuilds
                .Include(p => p.Details)
                .ThenInclude(d => d.Product)
                .OrderBy(p => p.PCBuildID)
                .ToListAsync();
        }

        public async Task<PCBuild?> GetByIdAsync(int id)
        {
            return await _context.PCBuilds
                .Include(p => p.Details)
                .ThenInclude(d => d.Product)
                .ThenInclude(prod => prod.Category)
                .FirstOrDefaultAsync(p => p.PCBuildID == id);
        }

        public async Task<PCBuild> CreateAsync(PCBuild pcBuild)
        {
            _context.PCBuilds.Add(pcBuild);
            await _context.SaveChangesAsync();
            return pcBuild;
        }

        public async Task<PCBuild> UpdateAsync(PCBuild pcBuild)
        {
            _context.Entry(pcBuild).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return pcBuild;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pcBuild = await _context.PCBuilds.FindAsync(id);
            if (pcBuild == null)
                return false;

            _context.PCBuilds.Remove(pcBuild);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PCBuilds.AnyAsync(p => p.PCBuildID == id);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PCBuilds.CountAsync();
        }
    }
}
