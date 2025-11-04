using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }

        public Customer? Customer { get; set; }
        public ICollection<OrderDetail>? Details { get; set; }
        public ICollection<OrderStatusHistory>? StatusHistory { get; set; }

        public ICollection<Payment>? Payments { get; set; }
    }
}
