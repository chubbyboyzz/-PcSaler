using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thường không cần nhưng tốt cho các thuộc tính đặc biệt

namespace PcSaler.DBcontext.Entites
{
    public class PCBuildDetail
    {
        [Key]
        public int PCBuildDetailID { get; set; }

        // Khóa ngoại tới Bảng PCBuild
        public int PCBuildID { get; set; }

        // Khóa ngoại tới Bảng Products
        public int ProductID { get; set; }

        // **THAY ĐỔI ĐÃ THÊM**
        // Trường xác định vai trò của sản phẩm trong cấu hình (VD: CPU, RAM_SLOT_1)
        [Required]
        [MaxLength(50)]
        public string ComponentType { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        // Navigation Properties

        // Quan hệ n-1 với PCBuild
        public PCBuild PCBuild { get; set; } = null!;

        // Quan hệ n-1 với Product
        public Product Product { get; set; } = null!;
    }
}