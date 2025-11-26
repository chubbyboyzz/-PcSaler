using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;

namespace PcSaler.Services
{
    public class LoginService 
    {
        private readonly ILoginService login;
        public LoginService(ILoginService login)
        {
            this.login = login;
        }
        public async Task<Customer?> LoginUserAsync(string username, string password)
        {
            var user = await login.GetUsersByUsername(username);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var hasher = new PasswordHasher<Customer>();

                if (user.PasswordHash.StartsWith("AQAAAA") || user.PasswordHash.StartsWith("$2"))
                {
                    try
                    {
                        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
                        if (result == PasswordVerificationResult.Success)
                        {
                            return user;
                        }
                    }
                    catch
                    {
                    }
                }
                if (user.PasswordHash == password)
                {
                    return user;
                }
            }
            return null;
        }
    }
}