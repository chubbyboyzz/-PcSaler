namespace PcSaler.Models
{
    public class ProductSearchInputViewModel
    {
        public string Category { get; set; }
        public string Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = decimal.MaxValue;

        // Dictionary chứa các bộ lọc động (Brand: Intel, Socket: AM5...)
        public Dictionary<string, string> DynamicAttributes { get; set; } = new Dictionary<string, string>();
    }
}
