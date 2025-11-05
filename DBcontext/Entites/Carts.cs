using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class Carts
    {
        [Key]
        public int CartID { get; set; }

        // Khóa ngoại tới Customer. Đây là UNIQUE trong DB (1 Customer có 1 Cart)
        public int CustomerID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties

        // Quan hệ 1-1 với Customer
        public Customer? Customer { get; set; }

        // Quan hệ 1-n với các mục trong giỏ hàng (CartItems)
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
