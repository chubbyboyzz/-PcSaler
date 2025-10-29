namespace PcSaler.DBcontext.Entites
{
    public class Product
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string Brand { get; set; }
        public string ProductName { get; set; }
        public string? Model { get; set; }
        public string? Specifications { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageURL { get; set; }
        public int WarrantyMonths { get; set; } = 12;
        public DateTime? ReleaseDate { get; set; }

        public Category Category { get; set; }

        //public bool IsAvailable { get; set; } = true;
    }
}
