using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;

public class ProductService : IProductService
{
    private readonly PCShopContext _db;

    public ProductService(PCShopContext db)
    {
        _db = db;
    }

    public List<CategoryProductViewModel> GetCategoryProducts(int? categoryId, string? query)
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

    public ProductListViewModel? GetProductDetails(int id)
    {
        var p = _db.Products
            .Include(x => x.Category)
            .FirstOrDefault(x => x.ProductID == id);

        if (p == null) return null;

        return new ProductListViewModel
        {
            ProductID = p.ProductID,
            ProductName = p.ProductName,
            CategoryName = p.Category?.CategoryName,
            ImageURL = p.ImageURL,
            Price = p.Price,
            Stock = p.Stock
        };
    }
    public List<Category> GetAllCategories()
    {
        return _db.Categories
            .OrderBy(c => c.CategoryName)
            .ToList();
    }
}
