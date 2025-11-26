namespace PcSaler.Models
{
    public class CartItemViewModel
    {
        public int ItemID { get; set; }

        public int Quantity { get; set; }

        // Loại sản phẩm: 'PRODUCT', 'PCBUILD', 'CUSTOMPC'
        public string ItemType { get; set; } = "PRODUCT";

        // --- Dữ liệu đầu ra (Output) để hiển thị lên Frontend ---
        // Các trường này có thể null khi Client gửi lên, 
        // nhưng sẽ được Repository điền dữ liệu khi trả về Client.
        public string? ProductName { get; set; }

        public decimal Price { get; set; }

        public string? ImageURL { get; set; }
    }
}
