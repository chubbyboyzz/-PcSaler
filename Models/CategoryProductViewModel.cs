namespace PcSaler.Models
{
    public class CategoryProductViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<ProductListViewModel> Products { get; set; } = new();
    }
}
