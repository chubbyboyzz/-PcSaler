using BCrypt.Net;
using PcSaler.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PcSaler.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 11;
        private readonly string _pepper;

        // Inject IConfiguration để lấy Pepper từ appsettings.json
        public BCryptPasswordHasher(IConfiguration configuration)
        {
            _pepper = configuration["SecuritySettings:Pepper"];

            //if (string.IsNullOrEmpty(_pepper))
            //{
            //    throw new Exception("Chưa cấu hình Pepper trong appsettings.json! Bảo mật đang gặp nguy hiểm.");
            //}
        }

        public string HashPassword(string password)
        {
            // BƯỚC 1: Pre-hash bằng HMAC-SHA256 với Pepper
            string pepperedPassword = ComputeHmac(password, _pepper);

            // BƯỚC 2: Hash bằng BCrypt
            return BCrypt.Net.BCrypt.HashPassword(pepperedPassword, WorkFactor);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                // Khi verify, ta cũng phải trộn Pepper vào password nhập vào
                // rồi mới đưa cho BCrypt kiểm tra
                string pepperedPassword = ComputeHmac(password, _pepper);

                return BCrypt.Net.BCrypt.Verify(pepperedPassword, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        // Hàm phụ trợ: Tạo HMAC-SHA256 (Chuẩn bảo mật cao hơn SHA256 thường)
        private string ComputeHmac(string rawData, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(rawData);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                // Chuyển sang Base64 để đảm bảo gọn gàng khi đưa vào BCrypt
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}