using PcSaler.DBcontext.Entites;

namespace PcSaler.Management_System.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByUsernameAsync(string username);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetTotalCountAsync();
    }
}
