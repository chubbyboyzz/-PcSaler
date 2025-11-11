using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly PCShopContext _context;

        public ProductRepository(PCShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductID)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            try
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return product;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(product.ProductID))
                {
                    throw new ArgumentException("Product not found");
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Handle foreign key constraint violations
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547)
                {
                    throw new InvalidOperationException("Không thể xóa sản phẩm này vì nó đang được sử dụng trong các đơn hàng hoặc build PC khác.");
                }
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ProductID == id);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            return await _context.Products.SumAsync(p => p.Price * p.Stock);
        }
    }
}
