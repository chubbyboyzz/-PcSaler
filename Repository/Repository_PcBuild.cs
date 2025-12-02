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
            return await _context.PCBuilds // Sửa tên bảng cho đúng với DBContext (PCBuild hoặc PCBuilds)
                .Select(x => new PCBuildDetailViewModel
                {
                    PCBuildID = x.PCBuildID,
                    PCBuildName = x.PCBuildName,
                    TotalPrice = x.TotalPrice, // Xử lý null
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
                    ImageURL = b.ImageURL,

                    Components = _context.PCBuildDetails
                        .Where(d => d.PCBuildID == b.PCBuildID)
                        .Select(d => new PCComponentViewModel
                        {
                            ProductID = d.ProductID,
                            ProductName = d.Product.ProductName,
                            ComponentType = d.ComponentType,
                            ImageURL = d.Product.ImageURL,
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