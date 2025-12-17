using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Repository
{
    public class Repository_PcBuild : IPcBuildService
    {
        private readonly PCShopContext _context;

        public Repository_PcBuild(PCShopContext context)
        {
            _context = context;
        }

        public async Task<List<PCBuildDetailViewModel>> GetAllPCBuild()
        {
            return await _context.PCBuilds
                .Select(x => new PCBuildDetailViewModel
                {
                    PCBuildID = x.PCBuildID,
                    PCBuildName = x.PCBuildName,
                    TotalPrice = x.TotalPrice,

                    // [SỬA] Vì DB không còn cột ImageURL, ta tạo đường dẫn ảo trỏ về Controller
                    // Controller "Home" phải có hàm "GetImage(int id)" trả về File Content
                    ImageURL = "/Home/GetImage/" + x.PCBuildID
                })
                .OrderByDescending(x => x.PCBuildID)
                .ToListAsync();
        }

        public async Task<PCBuildDetailViewModel?> GetPCBuildDetails(int id)
        {
            return await _context.PCBuilds
                .Where(b => b.PCBuildID == id)
                .Select(b => new PCBuildDetailViewModel
                {
                    PCBuildID = b.PCBuildID,
                    PCBuildName = b.PCBuildName,
                    Description = b.Description,
                    TotalPrice = b.TotalPrice,

                    // [SỬA] Đường dẫn ảnh cho bộ PC
                    ImageURL = "/Home/GetImage/" + b.PCBuildID,

                    // --- LẤY DANH SÁCH LINH KIỆN BÊN TRONG ---
                    Components = _context.PCBuildDetails
                        .Where(d => d.PCBuildID == b.PCBuildID)
                        .Select(d => new PCComponentViewModel
                        {
                            ProductID = d.ProductID,
                            ProductName = d.Product.ProductName,
                            ComponentType = d.ComponentType,

                            // [SỬA] Đường dẫn ảnh cho từng linh kiện (Product)
                            // Giả sử ông có ProductController với hàm GetImage tương tự
                            ImageURL = "/Product/GetImage/" + d.ProductID,

                            Quantity = d.Quantity,
                            UnitPrice = d.Product.Price
                        })
                        .OrderBy(c => c.ComponentType)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}