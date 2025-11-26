using PcSaler.DBcontext.Entites;

namespace PcSaler.Interfaces
{
    public interface ILoginService
    {
        Task<Customer?> GetUsersByID(int id);
        Task<Customer?> GetUsersByUsername(string Username);
        Task addAsync(Customer user);
        Task SaveChangeAsync();

    }
}
