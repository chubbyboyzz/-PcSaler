using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Repository
{
    public class Repository_Category : ICategoryService
    {
        private readonly PCShopContext _db;

        public Repository_Category(PCShopContext db)
        {
            _db = db;
        }

        public async Task<List<Categories>> GetAllCategories()
        {
            // Tạm thời giữ nguyên, nhưng nên dùng AsNoTracking() và ToListAsync()
            return await _db.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<List<CategoryProductViewModel>> GetCategoryProducts(int? categoryId, string? query)
        {
            IQueryable<Categories> queryable = _db.Categories.Include(c => c.Products);

            if (categoryId.HasValue && categoryId.Value > 0)
                queryable = queryable.Where(c => c.CategoryID == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(query))
                queryable = queryable.Where(c => c.Products.Any(p => p.ProductName.Contains(query)));

            return await queryable.Select(c => new CategoryProductViewModel
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                // **THAY ĐỔI CẦN THIẾT: Ánh xạ các trường mới**
                ComponentType = c.ComponentType,             // THÊM: Key cố định cho PC Builder
                IsRequiredForBuild = c.IsRequiredForBuild,   // THÊM: Xác định linh kiện bắt buộc
                // ***********************************************
                Products = c.Products
                    .OrderByDescending(p => p.ProductID)
                    .Select(p => new ProductListViewModel
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Price = p.Price,
                        ImageURL = p.ImageURL
                    }).ToList()
            }).ToListAsync(); // Sửa ToList() thành ToListAsync() để tuân thủ async
        }
    }
}