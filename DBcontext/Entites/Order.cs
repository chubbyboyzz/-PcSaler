using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcSaler.DBcontext.Entites
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; } = 0;

        // **PHẦN BỔ SUNG QUAN TRỌNG**
        public int CurrentStatusID { get; set; } // Khóa ngoại
        [ForeignKey("CurrentStatusID")]
        public OrderStatus CurrentStatus { get; set; } = null!; // Thuộc tính điều hướng
        // ------------------------------------

        // Navigation Properties
        public Customer? Customer { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<OrderStatusHistory>? StatusHistory { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
