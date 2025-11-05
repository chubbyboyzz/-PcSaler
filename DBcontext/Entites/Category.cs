using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần thiết cho [ForeignKey]

namespace PcSaler.DBcontext.Entites
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        // Thay thế SubCategory bằng ComponentType (Key cố định cho PC Builder)
        [Required]
        [MaxLength(100)]
        public string ComponentType { get; set; }

        // Thêm trường bắt buộc cho Build PC
        public bool IsRequiredForBuild { get; set; } = false;

        [MaxLength(255)]
        public string? Description { get; set; }

        // --- Thiết lập Phân cấp (Self-Referencing) ---

        // Khóa ngoại trỏ đến chính nó (NULLable)
        public int? ParentCategoryID { get; set; }

        // Thuộc tính điều hướng cho Category cha
        [ForeignKey("ParentCategoryID")]
        public Category? ParentCategory { get; set; }

        // Thuộc tính điều hướng cho các Category con
        public ICollection<Category>? Children { get; set; }

        // --- Quan hệ 1-n với Product ---

        // Collection các sản phẩm thuộc danh mục này
        public ICollection<Product>? Products { get; set; }
    }
}