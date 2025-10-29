namespace PcSaler.DBcontext.Entites
{
    public class OrderStatusHistory
    {
        public int HistoryID { get; set; }
        public int OrderID { get; set; }
        public int StatusID { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }

        public Order Order { get; set; }
        public OrderStatus Status { get; set; }
    }
}
