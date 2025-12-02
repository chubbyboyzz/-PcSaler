using PcSaler.Models;
using System.Threading.Tasks;

namespace PcSaler.Interfaces
{
    public interface ICustomerService
    {
        // Lấy thông tin profile theo ID
        Task<CustomerProfileViewModel?> GetProfileByIdAsync(int customerId);
    }
}