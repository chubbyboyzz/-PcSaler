using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;

namespace PcSaler.Repository
{
    public class Repository_Login : ILoginService
    {

        private readonly PCShopContext _connection;

        public Repository_Login(PCShopContext connection)
        {
            _connection = connection;
        }
        public async Task addAsync(Customer user)
        {
            await _connection.Customers.AddAsync(user);
        }
        public async Task<Customer?> GetUsersByID(int id)
        {
            return await _connection.Customers.FindAsync(id).AsTask();
        }
        public async Task<Customer?> GetUsersByUsername(string username)
        {
            return await _connection.Customers.FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task SaveChangeAsync()
        {
            await _connection.SaveChangesAsync();
        }
    }
     
}
