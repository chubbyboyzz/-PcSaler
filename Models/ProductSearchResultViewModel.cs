namespace PcSaler.Models
{
    public class ProductSearchResultViewModel
    {
        public object Products { get; set; } // List sản phẩm rút gọn
        public int TotalCount { get; set; }
        public object DynamicFilters { get; set; } // List bộ lọc động
        public object PriceRanges { get; set; }    // List khoảng giá
    }
}
