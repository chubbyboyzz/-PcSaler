namespace PcSaler.Services
{
    public interface IEmailSender
    {
        // Hàm gửi mail cơ bản: Nhận vào Email người nhận, Tiêu đề, và Nội dung
        Task SendEmailAsync(string email, string subject, string message);
    }
}