using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Models;

namespace PcSaler.Controllers
{
    public class PcBuildController : Controller
    {
        private readonly PCShopContext _db;

        public PcBuildController(PCShopContext db)
        {
            _db = db;
        }

        // GET: PcBuild
        public IActionResult Index()
        {
            try
            {
                var model = new ProductViewModel
                {
                    CPUs = _db.Products
                        .Where(p => p.Category != null && p.Category.ComponentType == "CPU")
                        .Select(p => new ProductListViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName,
                            CategoryName = p.Category.ComponentType,
                            Brand = p.Brand,
                            Model = p.Model,
                            Specifications = p.Specifications,
                            Price = p.Price,
                            Stock = p.Stock,
                            ImageURL = p.ImageURL,
                            WarrantyMonths = p.WarrantyMonths,
                            ReleaseDate = p.ReleaseDate
                        })
                        .ToList(),

                    Mainboards = _db.Products
                        .Where(p => p.Category != null && p.Category.ComponentType == "Mainboard")
                        .Select(p => new ProductListViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName,
                            CategoryName = p.Category.ComponentType,
                            Brand = p.Brand,
                            Model = p.Model,
                            Specifications = p.Specifications,
                            Price = p.Price,
                            Stock = p.Stock,
                            ImageURL = p.ImageURL,
                            WarrantyMonths = p.WarrantyMonths,
                            ReleaseDate = p.ReleaseDate
                        })
                        .ToList(),

                    RAMs = _db.Products
                        .Where(p => p.Category != null && p.Category.ComponentType == "RAM")
                        .Select(p => new ProductListViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName,
                            CategoryName = p.Category.ComponentType,
                            Brand = p.Brand,
                            Model = p.Model,
                            Specifications = p.Specifications,
                            Price = p.Price,
                            Stock = p.Stock,
                            ImageURL = p.ImageURL,
                            WarrantyMonths = p.WarrantyMonths,
                            ReleaseDate = p.ReleaseDate
                        })
                        .ToList(),

                    GPUs = _db.Products
                        .Where(p => p.Category != null && p.Category.ComponentType == "VGA")
                        .Select(p => new ProductListViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName,
                            CategoryName = p.Category.ComponentType,
                            Brand = p.Brand,
                            Model = p.Model,
                            Specifications = p.Specifications,
                            Price = p.Price,
                            Stock = p.Stock,
                            ImageURL = p.ImageURL,
                            WarrantyMonths = p.WarrantyMonths,
                            ReleaseDate = p.ReleaseDate
                        })
                        .ToList(),

                    PSUs = _db.Products
                        .Where(p => p.Category != null && p.Category.ComponentType == "PSU")
                        .Select(p => new ProductListViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName,
                            CategoryName = p.Category.ComponentType,
                            Brand = p.Brand,
                            Model = p.Model,
                            Specifications = p.Specifications,
                            Price = p.Price,
                            Stock = p.Stock,
                            ImageURL = p.ImageURL,
                            WarrantyMonths = p.WarrantyMonths,
                            ReleaseDate = p.ReleaseDate
                        })
                        .ToList(),

                    Cases = _db.Products
                        .Where(p => p.Category != null && p.Category.ComponentType == "Case")
                        .Select(p => new ProductListViewModel
                        {
                            ProductID = p.ProductID,
                            ProductName = p.ProductName,
                            CategoryName = p.Category.ComponentType,
                            Brand = p.Brand,
                            Model = p.Model,
                            Specifications = p.Specifications,
                            Price = p.Price,
                            Stock = p.Stock,
                            ImageURL = p.ImageURL,
                            WarrantyMonths = p.WarrantyMonths,
                            ReleaseDate = p.ReleaseDate
                        })
                        .ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi khi tải dữ liệu PC Build:");
                Console.WriteLine(ex.ToString());
                return Content("Đã xảy ra lỗi: " + ex.Message);
            }
        }
    }
}
