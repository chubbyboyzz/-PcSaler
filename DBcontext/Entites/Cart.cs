using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class Cart
    {
        [Key]
        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime AddedDate { get; set; } = DateTime.Now;

        public Customer? Customer { get; set; }
        public Product? Product { get; set; }
    }
}
