using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext; // Đảm bảo đúng tên namespace
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/products")] // Đổi Route thành "api/products"
public class ProductsController : ControllerBase
{
    private readonly PCShopContext _context;

    public ProductsController(PCShopContext context)
    {
        _context = context;
    }

    // Tên Model trả về cho gọn
    public class PriceRangeResult
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal min { get; set; }
        public decimal? max { get; set; }
    }

    public class DynamicFilterResult
    {
        public string name { get; set; }
        public ICollection<string> values { get; set; }
    }


    [HttpGet("search")] // Endpoint sẽ là: /api/products/search
    public async Task<IActionResult> SearchProducts()
    {
        // ===================================
        // 1. LẤY TẤT CẢ THAM SỐ TỪ QUERY
        // ===================================
        var queryParams = HttpContext.Request.Query;

        // Tham số cố định
        string category = queryParams.ContainsKey("category") ? queryParams["category"].ToString() : string.Empty;
        string search = queryParams.ContainsKey("search") ? queryParams["search"].ToString().ToLower() : string.Empty;
        int page = queryParams.ContainsKey("page") ? int.Parse(queryParams["page"]) : 1;
        int pageSize = queryParams.ContainsKey("pageSize") ? int.Parse(queryParams["pageSize"]) : 10;
        decimal minPrice = queryParams.ContainsKey("minPrice") ? decimal.Parse(queryParams["minPrice"]) : 0;
        decimal maxPrice = queryParams.ContainsKey("maxPrice") ? decimal.Parse(queryParams["maxPrice"]) : decimal.MaxValue;

        // ===================================
        // 2. XÂY DỰNG TRUY VẤN (IQUERYABLE)
        // ===================================

        // Bắt đầu với IQueryable - Rất quan trọng!
        // Nó không chạy truy vấn ngay, mà chỉ xây dựng câu SQL
        var query = _context.Products
                            .Include(p => p.Category) // Dùng Include để join bảng
                            .AsQueryable();

        // 2a. Lọc theo Category (bắt buộc)
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.ComponentType == category);
        }

        // 2b. Lọc theo Search (tên sản phẩm)
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.ProductName.ToLower().Contains(search));
        }

        // 2c. Lọc theo Giá
        query = query.Where(p => p.Price >= minPrice);
        if (maxPrice != decimal.MaxValue)
        {
            query = query.Where(p => p.Price <= maxPrice);
        }

        // 2d. LỌC ĐỘNG (Phần quan trọng nhất)
        // Lấy các filter động (Brand, Socket, Chipset...)
        var dynamicFilterKeys = queryParams.Keys
            .Where(k => k != "category" && k != "search" && k != "page"
                     && k != "pageSize" && k != "minPrice" && k != "maxPrice" && k != "_").ToList();

        foreach (var key in dynamicFilterKeys)
        {
            var value = queryParams[key].ToString();
            if (!string.IsNullOrEmpty(value))
            {
                // Thêm điều kiện: sản phẩm phải có thuộc tính (key) với giá trị (value)
                // CODE ĐÃ SỬA:
                query = query.Where(p => p.Attributes.Any(a => a.AttributeName == key && a.AttributeValue == value));
            }
        }

        // ===================================
        // 3. LẤY DỮ LIỆU BỘ LỌC (Filters)
        // ===================================

        // 3a. Lấy PriceRanges (Lọc theo danh mục)
        var categoryId = await _context.Categories
                                .Where(c => c.ComponentType == category)
                                .Select(c => (int?)c.CategoryID) // Ép kiểu int? (nullable)
                                .FirstOrDefaultAsync();

        var priceRangesQuery = _context.PriceRanges.AsQueryable();

        if (await priceRangesQuery.AnyAsync(r => r.CategoryID == categoryId))
        {
            // Nếu có bộ lọc riêng, dùng nó
            priceRangesQuery = priceRangesQuery.Where(r => r.CategoryID == categoryId);
        }
        else
        {
            // Nếu không, dùng bộ lọc Toàn Cục (NULL)
            priceRangesQuery = priceRangesQuery.Where(r => r.CategoryID == null);
        }

        var priceRanges = await priceRangesQuery
            .OrderBy(r => r.SortOrder)
            .Select(r => new PriceRangeResult
            {
                id = r.Identifier,
                name = r.DisplayName,
                min = r.MinPrice,
                max = r.MaxPrice
            })
            .ToListAsync();


        // 3b. Lấy DynamicFilters (Socket, Brand...)
        // Lấy TẤT CẢ thuộc tính có sẵn cho Category này (KHÔNG lọc theo giá hay search)
        var dynamicFilters = await _context.ProductAttributes
            .Where(a => a.Product.Category.ComponentType == category)
            .GroupBy(a => a.AttributeName) // Nhóm theo 'Brand', 'Socket'...
            .Select(g => new DynamicFilterResult
            {
                name = g.Key,
                values = g.Select(a => a.AttributeValue).Distinct().ToList()
            })
            .ToListAsync();


        // ===================================
        // 4. PHÂN TRANG VÀ THỰC THI
        // ===================================

        // Rất quan trọng: Lấy totalCount TRƯỚC khi phân trang
        var totalCount = await query.CountAsync();

        // Giờ mới phân trang
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new {
                productID = p.ProductID,
                productName = p.ProductName,
                brand = p.Brand,
                price = p.Price,
                imageURL = p.ImageURL
            })
            .ToListAsync(); // Chạy truy vấn SQL tại đây!

        // ===================================
        // 5. TRẢ VỀ KẾT QUẢ
        // ===================================
        return Ok(new
        {
            products = products,
            totalCount = totalCount,
            dynamicFilters = dynamicFilters,
            priceRanges = priceRanges
        });
    }
}