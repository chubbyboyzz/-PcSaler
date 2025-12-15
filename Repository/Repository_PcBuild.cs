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
                    // Giữ nguyên ImageURL của PCBuild (nếu bảng PCBuilds vẫn còn cột này)
                    // Nếu bảng PCBuilds cũng xóa cột ImageURL rồi thì ông sửa thành: ImageURL = "pc-setup.png"
                    ImageURL = x.ImageURL
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
                    ImageURL = b.ImageURL, // Ảnh đại diện của cả bộ PC

                    // --- LẤY DANH SÁCH LINH KIỆN BÊN TRONG ---
                    Components = _context.PCBuildDetails
                        .Where(d => d.PCBuildID == b.PCBuildID)
                        .Select(d => new PCComponentViewModel
                        {
                            ProductID = d.ProductID,
                            ProductName = d.Product.ProductName, // Lấy tên linh kiện
                            ComponentType = d.ComponentType,

                            // --- SỬA ĐOẠN NÀY ---
                            // CŨ: ImageURL = d.Product.ImageURL (Lỗi vì Product đã xóa ImageURL)
                            // MỚI: Trỏ về hàm GetImage lấy ảnh Binary của linh kiện đó
                            ImageURL = "/Product/GetImage/" + d.ProductID,
                            // --------------------

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