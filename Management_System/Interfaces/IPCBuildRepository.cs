using PcSaler.DBcontext.Entites;

namespace PcSaler.Management_System.Interfaces
{
    public interface IPCBuildRepository
    {
        Task<IEnumerable<PCBuild>> GetAllAsync();
        Task<PCBuild?> GetByIdAsync(int id);
        Task<PCBuild> CreateAsync(PCBuild pcBuild);
        Task<PCBuild> UpdateAsync(PCBuild pcBuild);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetTotalCountAsync();
    }
}
