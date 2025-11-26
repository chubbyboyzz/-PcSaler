using PcSaler.DBcontext.Entites;
using System.ComponentModel.DataAnnotations;

namespace PcSaler.Models
{
    public class CategoryProductViewModel
    {
        public int CategoryID { get; set; }
        [Required]
        [StringLength(100)]
        public required string CategoryName { get; set; }

        [Required]
        [StringLength(100)]
        public required string ComponentType { get; set; }

        public bool IsRequiredForBuild { get; set; }

        public int? ParentCategoryID { get; set; }

        // Danh sách các sản phẩm thuộc danh mục này
        public List<ProductListViewModel> Products { get; set; } = new();
    }
}