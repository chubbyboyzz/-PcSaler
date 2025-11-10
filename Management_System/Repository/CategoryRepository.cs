using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly PCShopContext _context;

        public CategoryRepository(PCShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categories>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.CategoryID)
                .ToListAsync();
        }

        public async Task<Categories?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
        }

        public async Task<Categories> CreateAsync(Categories category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Categories> UpdateAsync(Categories category)
        {
            try
            {
                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return category;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(category.CategoryID))
                {
                    throw new ArgumentException("Category not found");
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Handle foreign key constraint violations
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547)
                {
                    throw new InvalidOperationException("Không thể xóa danh mục này vì nó có sản phẩm hoặc danh mục con liên quan.");
                }
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryID == id);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Categories.CountAsync();
        }
    }
}
