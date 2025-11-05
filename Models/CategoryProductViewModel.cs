namespace PcSaler.Models
{
    public class CategoryProductViewModel
    {
        public int CategoryID { get; set; }

        // Tên danh mục hiển thị
        public string CategoryName { get; set; } = string.Empty;

        // **THAY ĐỔI ĐÃ THÊM**
        // Key cố định dùng để định danh slot (CPU, Mainboard, RAM)
        public string ComponentType { get; set; } = string.Empty;

        // Xác định xem linh kiện này có bắt buộc cho cấu hình PC hay không
        public bool IsRequiredForBuild { get; set; } = false;

        // Danh sách các sản phẩm thuộc danh mục này
        public List<ProductListViewModel> Products { get; set; } = new();
    }
}