using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class OrderStatusHistory
    {
        [Key]
        public int HistoryID { get; set; }
        public int OrderID { get; set; }
        public int StatusID { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }

        public Order Order { get; set; }
        public OrderStatus Status { get; set; }
    }
}
