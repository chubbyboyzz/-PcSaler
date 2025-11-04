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
        public async Task<List<Category>> GetAllCategories()
        {
            return _db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();
        }
        public async Task<List<CategoryProductViewModel>> GetCategoryProducts(int? categoryId, string? query)
        {
            IQueryable<Category> queryable = _db.Categories.Include(c => c.Products);

            if (categoryId.HasValue && categoryId.Value > 0)
                queryable = queryable.Where(c => c.CategoryID == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(query))
                queryable = queryable.Where(c => c.Products.Any(p => p.ProductName.Contains(query)));

            return queryable.Select(c => new CategoryProductViewModel
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                Products = c.Products
                    .OrderByDescending(p => p.ProductID)
                    .Select(p => new ProductListViewModel
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Price = p.Price,
                        ImageURL = p.ImageURL
                    }).ToList()
            }).ToList();
        }
    }
}
