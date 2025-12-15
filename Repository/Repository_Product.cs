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
            var query = _db.Products
                           .Include(p => p.Category)
                           .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Category.CategoryID == categoryId.Value);
            }

            return await query
                .Select(p => new ProductListViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Brand = p.Brand,
                    Model = p.Model,
                    Price = p.Price,
                    Stock = p.Stock,

                    // --- SỬA ĐOẠN NÀY ---
                    // CŨ: ImageURL = p.ImageURL (Lỗi vì không còn cột này)
                    // MỚI: Tự tạo đường dẫn trỏ về Controller
                    ImageURL = "/Product/GetImage/" + p.ProductID
                    // --------------------
                })
                .ToListAsync();
        }

        public async Task<ProductListViewModel?> GetProductDetails(int id)
        {
            // Kiểm tra tồn tại trước
            var p = _db.Products.Include(x => x.Category).FirstOrDefault(x => x.ProductID == id);
            if (p == null) return null;

            return await _db.Products
                .Where(p => p.ProductID == id)
                .Select(p => new ProductListViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Brand = p.Brand,
                    Model = p.Model,
                    Specifications = p.Specifications,
                    Price = p.Price,
                    WarrantyMonths = p.WarrantyMonths,
                    ReleaseDate = p.ReleaseDate,

                    // --- SỬA ĐOẠN NÀY ---
                    ImageURL = "/Product/GetImage/" + p.ProductID
                    // --------------------
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<ProductListViewModel>> GetProductQuery(string? query)
        {
            var term = query?.ToLower() ?? "";

            return await _db.Products
                .Where(p => p.ProductName.ToLower().Contains(term))
                .Select(p => new ProductListViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Price = p.Price,

                    // --- SỬA ĐOẠN NÀY ---
                    ImageURL = "/Product/GetImage/" + p.ProductID
                    // --------------------
                })
                .Take(5)
                .ToListAsync();
        }

        public async Task<string?> GetCategoryType(int categoryId)
        {
            return await _db.Categories
                .Where(c => c.CategoryID == categoryId)
                .Select(c => c.ComponentType)
                .FirstOrDefaultAsync();
        }

        public async Task<ProductSearchResultViewModel> SearchProductsAsync(ProductSearchInputViewModel input)
        {
            // 1. KHỞI TẠO QUERY
            var query = _db.Products
                           .Include(p => p.Category)
                           .AsQueryable();

            // 2. LỌC CỐ ĐỊNH
            if (!string.IsNullOrEmpty(input.Category))
                query = query.Where(p => p.Category.ComponentType == input.Category);

            if (!string.IsNullOrEmpty(input.Search))
                query = query.Where(p => p.ProductName.ToLower().Contains(input.Search));

            query = query.Where(p => p.Price >= input.MinPrice);

            if (input.MaxPrice < decimal.MaxValue)
                query = query.Where(p => p.Price <= input.MaxPrice);

            // 3. LỌC ĐỘNG
            if (input.DynamicAttributes != null && input.DynamicAttributes.Count > 0)
            {
                foreach (var item in input.DynamicAttributes)
                {
                    string key = item.Key;
                    string value = item.Value;
                    query = query.Where(p => p.Attributes.Any(a => a.AttributeName == key && a.AttributeValue == value));
                }
            }

            // 4. LẤY DỮ LIỆU BỔ TRỢ (Price Ranges)
            var categoryId = await _db.Categories
                                      .Where(c => c.ComponentType == input.Category)
                                      .Select(c => (int?)c.CategoryID)
                                      .FirstOrDefaultAsync();

            var priceRangesQuery = _db.PriceRanges.AsQueryable();

            if (await priceRangesQuery.AnyAsync(r => r.CategoryID == categoryId))
                priceRangesQuery = priceRangesQuery.Where(r => r.CategoryID == categoryId);
            else
                priceRangesQuery = priceRangesQuery.Where(r => r.CategoryID == null);

            var priceRanges = await priceRangesQuery
                .OrderBy(r => r.SortOrder)
                .Select(r => new
                {
                    id = r.Identifier,
                    name = r.DisplayName,
                    min = r.MinPrice,
                    max = r.MaxPrice
                }).ToListAsync();

            // 5. Dynamic Filter Options
            var dynamicFilters = await _db.ProductAttributes
                .Where(a => a.Product.Category.ComponentType == input.Category)
                .GroupBy(a => a.AttributeName)
                .Select(g => new
                {
                    name = g.Key,
                    values = g.Select(a => a.AttributeValue).Distinct().ToList()
                }).ToListAsync();

            // 6. PHÂN TRANG & KẾT QUẢ
            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((input.Page - 1) * input.PageSize)
                .Take(input.PageSize)
                .Select(p => new
                {
                    productID = p.ProductID,
                    productName = p.ProductName,
                    brand = p.Brand,
                    price = p.Price,

                    // --- SỬA ĐOẠN NÀY ---
                    // API Search trả về JSON nên cần chuỗi URL chuẩn
                    imageURL = "/Product/GetImage/" + p.ProductID
                    // --------------------
                }).ToListAsync();

            return new ProductSearchResultViewModel
            {
                Products = products,
                TotalCount = totalCount,
                DynamicFilters = dynamicFilters,
                PriceRanges = priceRanges
            };
        }
    }
}