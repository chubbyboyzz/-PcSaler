using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PcSaler.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config; // Inject Configuration để lấy AppPassword từ appsettings.json
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();

                // 1. Thông tin người gửi
                var senderEmail = _config["EmailSettings:SenderEmail"];
                var senderName = _config["EmailSettings:SenderName"] ?? "PCShop Support";
                emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));

                // 2. Thông tin người nhận
                emailMessage.To.Add(new MailboxAddress("", email));

                // 3. Nội dung Email
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = message; // Cho phép gửi HTML (in đậm, màu sắc...)
                emailMessage.Body = bodyBuilder.ToMessageBody();

                // 4. Kết nối SMTP Server và gửi
                using (var client = new SmtpClient())
                {
                    // Kết nối tới Gmail qua port 587 (TLS)
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                    // Đăng nhập bằng App Password
                    var appPassword = _config["EmailSettings:AppPassword"];
                    await client.AuthenticateAsync(senderEmail, appPassword);

                    // Bắn mail đi
                    await client.SendAsync(emailMessage);

                    // Ngắt kết nối sạch sẽ
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Nếu lỗi thì ghi log hoặc throw để Controller biết
                // Tạm thời throw để cậu debug cho dễ thấy lỗi
                throw new Exception($"Gửi mail thất bại: {ex.Message}");
            }
        }
    }
}