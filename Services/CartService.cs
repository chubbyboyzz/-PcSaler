using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Services
{
    public class CartService
    {
        private readonly ICartService _cartRepository;

        // Inject Repository vào Service
        public CartService(ICartService cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<bool> AddToCart(int customerId, CartItemViewModel item)
        {
            // 1. Lấy giỏ hàng từ Repo (Bao gồm cả các CartItems bên trong)
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);

            // 2. Nếu chưa có giỏ hàng -> Tạo mới
            if (cart == null)
            {
                cart = new Carts
                {
                    CustomerID = customerId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                await _cartRepository.CreateCartAsync(cart);
                await _cartRepository.SaveChangesAsync(); // Lưu để sinh CartID
            }

            // 3. LOGIC QUAN TRỌNG: Kiểm tra sản phẩm đã có trong giỏ chưa
            // (So sánh cả ItemID và ItemType để phân biệt Linh kiện vs PC Build)
            var inputType = item.ItemType.Trim().ToUpper();

            var existingItem = cart.CartItems?.FirstOrDefault(ci =>
                ci.ItemID == item.ItemID &&
                ci.ItemType.Trim().ToUpper() == inputType);

            if (existingItem != null)
            {
                // === TRƯỜNG HỢP A: ĐÃ TỒN TẠI -> CỘNG DỒN SỐ LƯỢNG ===

                // item.Quantity là số lượng khách gửi lên (thường là 1)
                // Dùng toán tử += để cộng dồn vào số cũ
                existingItem.Quantity += item.Quantity;

                // Lưu ý: Nếu bạn dùng logic "existingItem.Quantity = item.Quantity" thì nó sẽ là GHI ĐÈ (sai logic)
            }
            else
            {
                // === TRƯỜNG HỢP B: CHƯA TỒN TẠI -> THÊM DÒNG MỚI ===
                var newCartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ItemType = item.ItemType,
                    ItemID = item.ItemID,
                    Quantity = item.Quantity, // Lấy số lượng ban đầu (thường là 1)
                    AddedDate = DateTime.Now
                };
                await _cartRepository.AddCartItemAsync(newCartItem);
            }

            // 4. Lưu tất cả thay đổi xuống Database
            await _cartRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCount(int customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null || cart.CartItems == null) return 0;

            return cart.CartItems.Sum(ci => ci.Quantity);
        }

        public async Task<List<CartItemViewModel>> GetCartItems(int customerId)
        {
            // 1. Lấy dữ liệu thô từ Repo
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);

            // Kiểm tra null an toàn
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return new List<CartItemViewModel>();
            }

            // 2. Lấy danh sách ID sản phẩm
            var productIds = cart.CartItems
                .Where(x => x.ItemType == "PRODUCT")
                .Select(x => x.ItemID)
                .ToList();

            if (!productIds.Any()) return new List<CartItemViewModel>();

            // 3. Lấy thông tin chi tiết (Tên, Giá, Ảnh)
            var products = await _cartRepository.GetProductsByIdsAsync(productIds);

            // Chuyển sang Dictionary để tra cứu cho nhanh
            var productDict = products.ToDictionary(p => p.ProductID);

            var result = new List<CartItemViewModel>();

            // 4. Map dữ liệu (CÓ CHECK NULL ĐỂ TRÁNH LỖI 500)
            foreach (var item in cart.CartItems)
            {
                if (item.ItemType == "PRODUCT")
                {
                    // Kiểm tra xem sản phẩm này còn tồn tại trong kho không
                    if (productDict.TryGetValue(item.ItemID, out var product))
                    {
                        result.Add(new CartItemViewModel
                        {
                            ItemID = item.ItemID,
                            ItemType = item.ItemType,
                            Quantity = item.Quantity,
                            ProductName = product.ProductName,
                            Price = product.Price,
                            ImageURL = product.ImageURL
                        });
                    }
                    else
                    {
                        // Tình huống: Sản phẩm có trong giỏ nhưng đã bị xóa khỏi Database
                        // Ta có thể bỏ qua, hoặc thêm vào danh sách với ghi chú "Sản phẩm ngừng kinh doanh"
                        // Ở đây mình chọn bỏ qua để không gây lỗi hiển thị
                    }
                }
            }

            return result;
        }

        public async Task<bool> UpdateQuantity(int customerId, int itemId, string itemType, int quantity)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null || cart.CartItems == null) return false;

            var item = cart.CartItems.FirstOrDefault(x => x.ItemID == itemId && x.ItemType == itemType);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    _cartRepository.RemoveCartItem(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await _cartRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveItem(int customerId, int itemId, string itemType)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null || cart.CartItems == null) return false;

            var item = cart.CartItems.FirstOrDefault(x => x.ItemID == itemId && x.ItemType == itemType);

            if (item != null)
            {
                _cartRepository.RemoveCartItem(item);
                await _cartRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}