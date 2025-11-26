using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Models;
using System.Threading.Tasks;

namespace PcSaler.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly PCShopContext _context;

        public CustomerService(PCShopContext context)
        {
            _context = context;
        }

        public async Task<CustomerProfileViewModel?> GetProfileByIdAsync(int customerId)
        {
            // Truy vấn DB và map sang ViewModel
            var customer = await _context.Customers
                .Where(c => c.CustomerID == customerId)
                .Select(c => new CustomerProfileViewModel
                {
                    CustomerID = c.CustomerID,
                    FullName = c.FullName,
                    Username = c.Username,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    //CreatedAt = c.CreatedAt ?? DateTime.MinValue
                })
                .FirstOrDefaultAsync();

            return customer;
        }
    }
}