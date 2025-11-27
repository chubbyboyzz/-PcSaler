using System.ComponentModel.DataAnnotations;

namespace PcSaler.Models
{
    public class CheckoutViewModel
    {
        // === THÔNG TIN GIAO HÀNG ===
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }

        public string? Note { get; set; }

        // === THÔNG TIN THANH TOÁN ===
        public string PaymentMethod { get; set; } = "COD"; // Mặc định

        // Các trường giả lập (Không bắt buộc Required vì COD không cần)
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCVV { get; set; }
        public string? PaypalEmail { get; set; }

        // === DỮ LIỆU HIỂN THỊ ===
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
    }
}