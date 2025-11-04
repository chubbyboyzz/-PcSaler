namespace PcSaler.Models
{
    public class ProductListViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? ImageURL { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsHotDeal { get; set; } = false;
    }
}
