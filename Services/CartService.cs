using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Services
{
    public class CartService
    {
        private readonly ICartService _cartRepository;

        public CartService(ICartService cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<bool> AddToCart(int customerId, CartItemViewModel item)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);

            if (cart == null)
            {
                cart = new Carts
                {
                    CustomerID = customerId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                await _cartRepository.CreateCartAsync(cart);
                await _cartRepository.SaveChangesAsync();
            }

            var inputType = item.ItemType.Trim().ToUpper();
            var existingItem = cart.CartItems?.FirstOrDefault(ci =>
                ci.ItemID == item.ItemID &&
                ci.ItemType.Trim().ToUpper() == inputType);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ItemType = item.ItemType,
                    ItemID = item.ItemID,
                    Quantity = item.Quantity,
                    AddedDate = DateTime.Now
                };
                await _cartRepository.AddCartItemAsync(newCartItem);
            }

            await _cartRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCount(int customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null || cart.CartItems == null) return 0;
            return cart.CartItems.Sum(ci => ci.Quantity);
        }

        // --- [FIXED] GET CART (DB) ---
        public async Task<List<CartItemViewModel>> GetCartItems(int customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return new List<CartItemViewModel>();
            }

            // Lấy list ID để tra cứu thông tin sản phẩm
            var productIds = cart.CartItems.Select(x => x.ItemID).Distinct().ToList();
            var products = await _cartRepository.GetProductsByIdsAsync(productIds);
            var productDict = products.ToDictionary(p => p.ProductID);

            var result = new List<CartItemViewModel>();

            foreach (var item in cart.CartItems)
            {
                // Logic: Nếu tìm thấy trong bảng Product thì lấy thông tin chi tiết
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
                    
                    result.Add(new CartItemViewModel
                    {
                        ItemID = item.ItemID,
                        ItemType = item.ItemType, // Giữ nguyên Type quan trọng này
                        Quantity = item.Quantity,
                        ProductName = !string.IsNullOrEmpty(item.ItemType) ? item.ItemType : "Cấu hình PC",
                        Price = 0, // Giá item ảo là 0
                        ImageURL = "pc-setup.png" // Ảnh dummy
                    });
                }
            }
            return result;
        }

        // --- [FIXED] GET GUEST CART (LocalStorage) ---
        public async Task<List<CartItemViewModel>> GetGuestCartItems(List<CartItemViewModel> guestItems)
        {
            if (guestItems == null || !guestItems.Any()) return new List<CartItemViewModel>();

            var productIds = guestItems.Select(x => x.ItemID).Distinct().ToList();
            var products = await _cartRepository.GetProductsByIdsAsync(productIds);
            var productDict = products.ToDictionary(p => p.ProductID);

            var result = new List<CartItemViewModel>();

            foreach (var item in guestItems)
            {
                // Tìm thấy trong DB thì update thông tin mới nhất
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
                    
                    result.Add(item);
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
                if (quantity <= 0) _cartRepository.RemoveCartItem(item);
                else item.Quantity = quantity;
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