namespace PcSaler.DBcontext.Entites
{
    public class OrderStatus
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public string? Description { get; set; }

        public ICollection<OrderStatusHistory>? History { get; set; }
    }
}
