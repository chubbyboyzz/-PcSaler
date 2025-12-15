using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Models;
using PcSaler.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PcSaler.Repository
{
    public class Repository_CustomersPC : ICustomerPCService
    {
        private readonly PCShopContext _context;

        public Repository_CustomersPC(PCShopContext context)
        {
            _context = context;
        }

        // 1. LẤY HOẶC KHỞI TẠO 3 SLOT CHO KHÁCH
        public async Task<List<PCBuildDetailViewModel>> GetUserSlots(int customerId)
        {
            // 1. Lấy dữ liệu từ DB (Giữ nguyên)
            var slots = await _context.CustomPCs
                .Where(c => c.CustomerID == customerId && c.BuildName.StartsWith("Cấu hình #"))
                .Include(c => c.Details)
                .ThenInclude(d => d.Product)
                .OrderBy(c => c.BuildName)
                .ToListAsync();

            // 2. Logic tạo mới slot nếu chưa có (Giữ nguyên)
            for (int i = 1; i <= 3; i++)
            {
                string slotName = $"Cấu hình #{i}";
                if (!slots.Any(s => s.BuildName == slotName))
                {
                    var newSlot = new CustomPC
                    {
                        CustomerID = customerId,
                        BuildName = slotName,
                        Description = "Cấu hình đang xây dựng",
                        TotalPrice = 0,
                        CreatedAt = DateTime.Now,
                        Details = new List<CustomPCDetail>()
                    };
                    _context.CustomPCs.Add(newSlot);
                    await _context.SaveChangesAsync();
                    slots.Add(newSlot);
                }
            }

            // 3. MAP DỮ LIỆU
            return slots.Select(s => new PCBuildDetailViewModel
            {
                PCBuildID = s.CustomPCID,
                PCBuildName = s.BuildName,
                TotalPrice = s.TotalPrice,
                Description = s.Description,
                ImageURL = "custom-pc-placeholder.png", // Ảnh đại diện slot (giữ nguyên ảnh tĩnh)

                Components = (s.Details ?? new List<CustomPCDetail>()).Select(d => new PCComponentViewModel
                {
                    ComponentType = d.ComponentType,
                    ProductID = d.ProductID,
                    ProductName = d.Product?.ProductName ?? "Sản phẩm đã ngừng kinh doanh",
                    UnitPrice = d.Product?.Price ?? 0,

                    // --- SỬA ĐOẠN NÀY ---
                    // CŨ: ImageURL = d.Product?.ImageURL ?? "no-img.png"
                    // MỚI: Trỏ về Controller lấy ảnh Binary
                    ImageURL = "/Product/GetImage/" + d.ProductID,
                    // --------------------

                    Quantity = d.Quantity
                }).ToList()

            }).OrderBy(s => s.PCBuildName).ToList();
        }

        // 2. AUTO-SAVE: CẬP NHẬT LINH KIỆN VÀO SLOT
        public async Task UpdateSlotItem(int customerId, int customPcId, string componentType, int productId)
        {
            var slot = await _context.CustomPCs
                .Include(x => x.Details)
                .FirstOrDefaultAsync(c => c.CustomPCID == customPcId && c.CustomerID == customerId);

            if (slot == null) throw new Exception("Không tìm thấy cấu hình!");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Sản phẩm không tồn tại");

            var detail = slot.Details.FirstOrDefault(d => d.ComponentType == componentType);

            if (detail != null)
            {
                detail.ProductID = productId;
            }
            else
            {
                _context.CustomPCDetails.Add(new CustomPCDetail
                {
                    CustomPCID = customPcId,
                    ComponentType = componentType,
                    ProductID = productId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();

            var newTotal = await _context.CustomPCDetails
                .Where(d => d.CustomPCID == customPcId)
                .SumAsync(d => d.Product.Price * (d.Quantity));

            slot.TotalPrice = newTotal;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveSlotItem(int customerId, int customPcId, string componentType)
        {
            var detail = await _context.CustomPCDetails
                .Include(d => d.CustomPC)
                .FirstOrDefaultAsync(d => d.CustomPCID == customPcId
                                        && d.CustomPC.CustomerID == customerId
                                        && d.ComponentType == componentType);

            if (detail != null)
            {
                var price = await _context.Products.Where(p => p.ProductID == detail.ProductID).Select(p => p.Price).FirstOrDefaultAsync();
                detail.CustomPC.TotalPrice -= price * (detail.Quantity);

                _context.CustomPCDetails.Remove(detail);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddToCartFromSlot(int customerId, int slotId)
        {
            // 1. Lấy Slot hiện tại (Kèm linh kiện)
            var slot = await _context.CustomPCs
                .Include(c => c.Details)
                .FirstOrDefaultAsync(c => c.CustomPCID == slotId && c.CustomerID == customerId);

            if (slot == null || !slot.Details.Any())
                throw new Exception("Cấu hình trống, vui lòng chọn linh kiện!");

            // 2. Lấy Giỏ hàng của khách
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            // Nếu chưa có giỏ -> Tạo mới
            if (cart == null)
            {
                cart = new Carts
                {
                    CustomerID = customerId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // 3. DUYỆT TỪNG LINH KIỆN ĐỂ MERGE VÀO GIỎ
            foreach (var detail in slot.Details)
            {
                var existingItem = cart.CartItems?
                    .FirstOrDefault(ci => ci.ItemID == detail.ProductID
                                       && ci.ItemType == "PRODUCT");

                if (existingItem != null)
                {
                    existingItem.Quantity += detail.Quantity;
                }
                else
                {
                    // THÊM MỚI
                    var newItem = new CartItem
                    {
                        CartID = cart.CartID,
                        ItemType = "PRODUCT",
                        ItemID = detail.ProductID,
                        Quantity = detail.Quantity,
                        AddedDate = DateTime.Now
                    };
                    _context.CartItems.Add(newItem);
                }
            }
            _context.CustomPCDetails.RemoveRange(slot.Details);

            // Reset giá tiền slot về 0
            slot.TotalPrice = 0;

            // 5. LƯU TẤT CẢ THAY ĐỔI
            await _context.SaveChangesAsync();
        }

    }
}