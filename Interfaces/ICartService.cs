using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface ICartService
    {
        // Lấy giỏ hàng của khách (kèm theo các món bên trong)
        Task<Carts?> GetCartByCustomerIdAsync(int customerId);

        // Tạo giỏ hàng mới
        Task CreateCartAsync(Carts cart);

        // Thêm một dòng chi tiết vào giỏ
        Task AddCartItemAsync(CartItem item);

        // Xóa một dòng chi tiết
        void RemoveCartItem(CartItem item);

        // Lấy danh sách thông tin sản phẩm dựa trên List ID (để hiển thị tên, giá, ảnh)
        Task<List<Product>> GetProductsByIdsAsync(List<int> productIds);

        // Lưu thay đổi vào DB
        Task SaveChangesAsync();
    }
}
