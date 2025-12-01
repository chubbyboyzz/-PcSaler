using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;

namespace PcSaler.Services
{
    public class LoginService
    {
        private readonly ILoginService _repo;
        private readonly IPasswordHasher _hasher;

        public LoginService(ILoginService repo, IPasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        // --- 1. LOGIC ĐĂNG NHẬP CHÍNH (Giữ nguyên của ông) ---
        public async Task<Customer?> LoginUserAsync(string username, string password)
        {
            var user = await _repo.GetUsersByUsername(username);
            if (user == null) return null;

            // Nếu phát hiện đây là tài khoản Google (dựa vào chuỗi đặc biệt này)
            // Thì TỪ CHỐI NGAY LẬP TỨC, không cần gọi BCrypt 
            if (user.PasswordHash == "GOOGLE_AUTH_NO_PASSWORD" || string.IsNullOrEmpty(user.PasswordHash))
            {
                return null; // Bắt buộc phải đăng nhập bằng nút Google
            }
            // Nếu user Google (không có pass) thì chặn login bằng mật khẩu
            if (string.IsNullOrEmpty(user.PasswordHash)) return null;

            // Check pass
            if (_hasher.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        // Hàm tìm User bằng Email (Dùng cho Google Login)
        public async Task<Customer?> GetUsersByEmail(string email)
        {
            return await _repo.GetUsersByEmail(email);
        }

        // Hàm thêm mới User (Dùng khi Auto Register)
        public async Task addAsync(Customer user)
        {
            await _repo.addAsync(user);
        }

        // Hàm lưu vào DB
        public async Task SaveChangeAsync()
        {
            await _repo.SaveChangeAsync();
        }
    }
}