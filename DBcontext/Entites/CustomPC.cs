using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class CustomPC
    {
        [Key]
        public int CustomPCID { get; set; }
        public int CustomerID { get; set; }
        public string? BuildName { get; set; }
        public string? Description { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Customer? Customer { get; set; }
        public ICollection<CustomPCDetail>? Details { get; set; }
    }
}
