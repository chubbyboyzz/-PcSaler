using PcSaler.DBcontext.Entites;

namespace PcSaler.Interfaces
{
    public interface ILoginService
    {
        Task<Customer?> GetUsersByID(int id);
        Task<Customer?> GetUsersByUsername(string Username);
        Task addAsync(Customer user);
        Task SaveChangeAsync();
        Task<Customer?> GetUsersByEmail(string email);
        Task<string> CreateOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task UpdatePasswordAsync(string email, string newPasswordHash);

    }
}
