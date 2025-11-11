using PcSaler.DBcontext.Entites;

namespace PcSaler.Management_System.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Categories>> GetAllAsync();
        Task<Categories?> GetByIdAsync(int id);
        Task<Categories> CreateAsync(Categories category);
        Task<Categories> UpdateAsync(Categories category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetTotalCountAsync();
    }
}
