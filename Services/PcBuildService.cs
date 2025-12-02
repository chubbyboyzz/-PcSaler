using PcSaler.Interfaces;
using PcSaler.Models;
using PcSaler.Repository;

namespace PcSaler.Services
{
    public class PcBuildService 
    {
        private readonly IPcBuildService _pcBuild;

        public PcBuildService(IPcBuildService pcBuild)
        {
            _pcBuild = pcBuild;
        }
        // Bỏ tham số 'int id' vì lấy tất cả thì không cần id
        public async Task<List<PCBuildDetailViewModel>> GetAllPCBuild()
        {
            return await _pcBuild.GetAllPCBuild();
        }

        public async Task<PCBuildDetailViewModel?> GetPCBuildDetails(int id)
        {
            return await _pcBuild.GetPCBuildDetails(id);
        }
    }
}