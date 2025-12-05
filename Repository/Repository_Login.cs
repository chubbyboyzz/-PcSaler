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
        public async Task<Customer?> GetUsersByEmail(string email)
        {
            return await _connection.Customers.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            // 1. Lấy User ID
            var user = await _connection.Customers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // 2. Tìm mã OTP khớp với User, chưa dùng, và đúng mã
            var token = await _connection.VerificationTokens
                .Where(t => t.CustomerID == user.CustomerID
                         && t.TokenCode == otp
                         && !t.IsUsed
                         && t.TokenType == "RESET_PASSWORD")
                .OrderByDescending(t => t.CreatedAt) // Lấy cái mới nhất
                .FirstOrDefaultAsync();

            if (token == null) return false; // Mã sai hoặc không tồn tại

            // 3. Kiểm tra hết hạn
            if (token.ExpiresAt < DateTime.Now) return false; // Hết hạn rồi

            // 4. Nếu đúng hết -> Đánh dấu là đã dùng (để không dùng lại lần 2)
            token.IsUsed = true;
            await _connection.SaveChangesAsync();

            return true;
        }

        public async Task UpdatePasswordAsync(string email, string newPasswordHash)
        {
            var user = await _connection.Customers.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.PasswordHash = newPasswordHash;
                await _connection.SaveChangesAsync();
            }
        }
        public async Task<string> CreateOtpAsync(string email)
        {
            // 1. Tìm user theo email
            var user = await _connection.Customers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null; // Email không tồn tại

            // 2. Tạo mã OTP ngẫu nhiên (6 số)
            var otpCode = new Random().Next(100000, 999999).ToString();

            // 3. (Optional) Vô hiệu hóa các mã cũ chưa dùng của user này (Dọn rác)
            var oldTokens = _connection.VerificationTokens
                .Where(t => t.CustomerID == user.CustomerID && !t.IsUsed);
            foreach (var token in oldTokens)
            {
                token.IsUsed = true; // Hủy mã cũ
            }

            // 4. Tạo bản ghi Token mới
            var newToken = new VerificationToken
            {
                CustomerID = user.CustomerID,
                TokenCode = otpCode,
                TokenType = "RESET_PASSWORD",
                ExpiresAt = DateTime.Now.AddMinutes(10), // Hết hạn sau 10 phút
                IsUsed = false,
                CreatedAt = DateTime.Now
            };



            await _connection.VerificationTokens.AddAsync(newToken);
            await _connection.SaveChangesAsync();

            return otpCode; // Trả về mã để Service mang đi gửi mail
        }
    }
     
}
