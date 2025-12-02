using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites; // Giữ nguyên typo 'Entites' theo project của bạn
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Repository
{
    public class Repository_Cart : ICartService // Đảm bảo interface này có khai báo đủ các hàm bên dưới
    {
        private readonly PCShopContext _context;

        public Repository_Cart(PCShopContext context)
        {
            _context = context;
        }

        public async Task<Carts?> GetCartByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                         .Include(c => c.CartItems) // Quan trọng: Lấy kèm items
                         .FirstOrDefaultAsync(c => c.CustomerID == customerId);
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, string itemType, int itemId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartID == cartId && ci.ItemType == itemType && ci.ItemID == itemId);
        }

        public async Task<bool> IsItemExistsAsync(string itemType, int itemId)
        {
            if (itemType == "PRODUCT") return await _context.Products.AnyAsync(p => p.ProductID == itemId);
            // Lưu ý: Kiểm tra lại tên bảng PCBuilds hay PCBuild trong DBContext của bạn
            if (itemType == "PCBUILD") return await _context.PCBuilds.AnyAsync(p => p.PCBuildID == itemId);
            if (itemType == "CUSTOMPC") return await _context.CustomPCs.AnyAsync(p => p.CustomPCID == itemId);
            return false;
        }

        public async Task CreateCartAsync(Carts cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        public async Task AddCartItemAsync(CartItem item)
        {
            await _context.CartItems.AddAsync(item);
        }

        public void UpdateCartItem(CartItem item)
        {
            _context.CartItems.Update(item);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // --- ĐÃ SỬA LỖI NotImplementedException ---
        public void RemoveCartItem(CartItem item)
        {
            _context.CartItems.Remove(item);
        }

        // --- ĐÃ SỬA LỖI NotImplementedException ---
        public async Task<List<Product>> GetProductsByIdsAsync(List<int> productIds)
        {
            return await _context.Products
                         .Where(p => productIds.Contains(p.ProductID))
                         .ToListAsync();
        }
    }
}