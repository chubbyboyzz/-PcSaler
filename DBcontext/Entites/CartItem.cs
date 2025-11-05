using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcSaler.DBcontext.Entites
{
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        // Khóa ngoại tới Giỏ hàng tổng quát (Carts)
        public int CartID { get; set; }

        // Loại mặt hàng: 'PRODUCT', 'PCBUILD', 'CUSTOMPC'
        [Required]
        [MaxLength(50)]
        public string ItemType { get; set; } = null!;

        // ID của mặt hàng (ProductID, PCBuildID, hoặc CustomPCID)
        public int ItemID { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navigation Properties

        // Quan hệ n-1 với Carts
        public Carts? Cart { get; set; }

        /* * LƯU Ý: Không thể sử dụng Product?, PCBuild?, CustomPC? 
        * trực tiếp trong Entity này vì ItemID có thể trỏ đến 3 bảng khác nhau. 
        * Logic này phải được xử lý ở tầng Application Logic (Service/Repository).
        */
    }
}