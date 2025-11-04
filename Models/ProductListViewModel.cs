namespace PcSaler.Models
{
    public class ProductListViewModel
    {
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryName { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Specifications { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageURL { get; set; }
        public int WarrantyMonths { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
