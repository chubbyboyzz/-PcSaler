using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface IPcBuildService
    {
        Task<List<PCBuildDetailViewModel>>? GetAllPCBuild();
        Task<PCBuildDetailViewModel>? GetPCBuildDetails(int id);
    }
}
