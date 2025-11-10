using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Services
{
    public class PCBuildManagementService
    {
        private readonly IPCBuildRepository _pcBuildRepository;

        public PCBuildManagementService(IPCBuildRepository pcBuildRepository)
        {
            _pcBuildRepository = pcBuildRepository;
        }

        public async Task<IEnumerable<PCBuild>> GetAllPCBuildsAsync()
        {
            return await _pcBuildRepository.GetAllAsync();
        }

        public async Task<PCBuild?> GetPCBuildByIdAsync(int id)
        {
            return await _pcBuildRepository.GetByIdAsync(id);
        }

        public async Task<PCBuild> CreatePCBuildAsync(PCBuild pcBuild)
        {
            return await _pcBuildRepository.CreateAsync(pcBuild);
        }

        public async Task<PCBuild> UpdatePCBuildAsync(PCBuild pcBuild)
        {
            return await _pcBuildRepository.UpdateAsync(pcBuild);
        }

        public async Task<bool> DeletePCBuildAsync(int id)
        {
            return await _pcBuildRepository.DeleteAsync(id);
        }

        public async Task<bool> PCBuildExistsAsync(int id)
        {
            return await _pcBuildRepository.ExistsAsync(id);
        }

        public async Task<int> GetTotalPCBuildsAsync()
        {
            return await _pcBuildRepository.GetTotalCountAsync();
        }
    }
}
