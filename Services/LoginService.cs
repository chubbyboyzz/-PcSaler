using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;

namespace PcSaler.Services
{
    public class LoginService
    {
        private readonly ILoginService _repo;
        private readonly IPasswordHasher _hasher;
        private readonly IEmailSender _emailSender;

        public LoginService(ILoginService repo, IPasswordHasher hasher, IEmailSender emailSender)
        {
            _repo = repo;
            _hasher = hasher;
            _emailSender = emailSender;
        }

        // --- 1. LOGIC ĐĂNG NHẬP CHÍNH  ---
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

        public async Task<bool> SendForgotPasswordEmailAsync(string email)
        {
            // Bước 1: Gọi Repo để tạo và lưu OTP vào DB
            var otpCode = await _repo.CreateOtpAsync(email);

            if (string.IsNullOrEmpty(otpCode))
            {
                return false; // Email không tồn tại
            }

            // Bước 2: Soạn nội dung Email
            string subject = "[PCShop] Xác nhận đặt lại mật khẩu";
            string message = $@"
            <h3>Yêu cầu đặt lại mật khẩu</h3>
            <p>Bạn vừa yêu cầu lấy lại mật khẩu tại PCShop.</p>
            <p>Mã xác nhận của bạn là: <b style='color:red; font-size: 20px;'>{otpCode}</b></p>
            <p>Mã này có hiệu lực trong vòng <b>10 phút</b>.</p>
            <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

            // Bước 3: Gửi mail
            await _emailSender.SendEmailAsync(email, subject, message);

            return true; // Thành công
        }
        // Hàm tìm User bằng Email (Dùng cho Google Login)
        public async Task<Customer?> GetUsersByEmail(string email)
        {
            return await _repo.GetUsersByEmail(email);
        }

        // Hàm xác thực OTP
        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            return await _repo.VerifyOtpAsync(email, otp);
        }

        // 2. Hàm đổi mật khẩu (Hash xong mới đẩy xuống Repo)
        public async Task ResetPasswordAsync(string email, string newPassword)
        {
            // Hash mật khẩu mới trước khi lưu
            string secureHash = _hasher.HashPassword(newPassword);

            // Gọi Repo lưu
            await _repo.UpdatePasswordAsync(email, secureHash);
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

        // hàm lấy User bằng Username
        public async Task<Customer?> GetUsersByUsername(string username)
        {
            // Gọi xuống Repository để lấy dữ liệu
            return await _repo.GetUsersByUsername(username);
        }
    }
}