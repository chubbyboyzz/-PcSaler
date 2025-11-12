using PcSaler.DBcontext.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcSaler.Models
{
    public class ProductAttributeViewModel
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

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}
