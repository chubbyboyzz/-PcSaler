namespace PcSaler.Models
{
    public class PCComponentViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ComponentType { get; set; } // CPU, VGA, RAM...
        public string ImageURL { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => UnitPrice * Quantity; // Tự tính thành tiền
    }
}
