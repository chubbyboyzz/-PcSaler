using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Repository
{
    public class Repository_Product : IProductService
    {
        private readonly PCShopContext _db;

        public Repository_Product(PCShopContext db)
        {
            _db = db;
        }
        public async Task<List<ProductListViewModel>> GetProductsByCategory(int? categoryId)
        {
            // Bắt đầu với IQueryable
            var query = _db.Products
                           .Include(p => p.Category) // Cần thiết để truy cập p.Category.CategoryName
                           .AsQueryable();

            // === SỬA LỖI KIỂM TRA ĐIỀU KIỆN VÀ LỌC ===
            // 1. Kiểm tra nếu categoryId có giá trị (không null)
            if (categoryId.HasValue)
            {
                // 2. Lọc trực tiếp bằng số nguyên (int)
                // Không cần String.IsNullOrEmpty nữa.
                query = query.Where(p => p.Category.CategoryID == categoryId.Value);
            }
            // =========================================

            return await query
                .Select(p => new ProductListViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Brand = p.Brand,
                    Model = p.Model,
                    Price = p.Price,
                    ImageURL = p.ImageURL,
                })
                .ToListAsync();
        }
        public async Task<ProductListViewModel?> GetProductDetails(int id)
        {
            var p = _db.Products
                .Include(x => x.Category)
                .FirstOrDefault(x => x.ProductID == id);

            if (p == null) return null;

            return await _db.Products
                .Where(p => p.ProductID == id)
                .Select(p => new ProductListViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    // CategoryName được lấy thông qua navigation property
                    CategoryName = p.Category.CategoryName,
                    Brand = p.Brand,
                    Model = p.Model,
                    Specifications = p.Specifications,
                    Price = p.Price,
                    ImageURL = p.ImageURL,
                    WarrantyMonths = p.WarrantyMonths,
                    ReleaseDate = p.ReleaseDate
                })
                .FirstOrDefaultAsync();
        }
    }
}
