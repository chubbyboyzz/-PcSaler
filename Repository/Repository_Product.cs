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
