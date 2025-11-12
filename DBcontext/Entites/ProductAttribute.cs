using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcSaler.DBcontext.Entites
{
    public class ProductAttribute
    {
        [Key]
        public int AttributeID { get; set; }

        public int ProductID { get; set; }

        [Required]
        [StringLength(100)]
        public required string AttributeName { get; set; }

        [Required]
        [StringLength(255)]
        public required string AttributeValue { get; set; }

        // Mối quan hệ: Một Attribute thuộc về một Product
        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}
