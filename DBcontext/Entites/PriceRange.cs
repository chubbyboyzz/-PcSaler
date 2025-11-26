using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcSaler.DBcontext.Entites // Đảm bảo namespace này là đúng
{
    [Table("PriceRanges")] // Khai báo tên bảng
    public class PriceRange
    {
        [Key] // Khóa chính
        public int RangeID { get; set; }

        [Required]
        [StringLength(50)]
        public string Identifier { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxPrice { get; set; } // Dùng decimal? để cho phép NULL

        public int SortOrder { get; set; }

        // Khóa ngoại (cho phép NULL)
        public int? CategoryID { get; set; }

        // Thuộc tính điều hướng (Navigation Property)
        [ForeignKey("CategoryID")]
        public virtual Categories Category { get; set; }
    }
}