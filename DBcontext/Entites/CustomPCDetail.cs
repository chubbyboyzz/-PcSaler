using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thường không cần nhưng tốt cho các thuộc tính đặc biệt

namespace PcSaler.DBcontext.Entites
{
    public class CustomPCDetail
    {
        [Key]
        public int CustomPCDetailID { get; set; }

        // Khóa ngoại tới Bảng CustomPC
        public int CustomPCID { get; set; }

        // Khóa ngoại tới Bảng Products
        public int ProductID { get; set; }

        // **THAY ĐỔI ĐÃ THÊM**
        // Trường xác định vai trò của sản phẩm trong cấu hình (VD: CPU, RAM_SLOT_1).
        // Phải là NOT NULL theo DB structure.
        [Required]
        [MaxLength(50)]
        public string ComponentType { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        [MaxLength(255)]
        public string? Note { get; set; }

        // Navigation Properties

        // Quan hệ n-1 với CustomPC
        public CustomPC? CustomPC { get; set; }

        // Quan hệ n-1 với Product
        public Product? Product { get; set; }

    }
}