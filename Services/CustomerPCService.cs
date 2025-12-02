using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Services
{
    public class CustomerPCService
    {
        private readonly ICustomerPCService _customerPCService;
        public CustomerPCService(ICustomerPCService customerPCRepo)
        {
            _customerPCService = customerPCRepo;
        }
        public async Task<List<PCBuildDetailViewModel>> GetUserSlots(int customerId)
        {
            return await _customerPCService.GetUserSlots(customerId);
        }
        public async Task UpdateSlotItem(int customerId, int customPcId, string componentType, int productId)
        {
            await _customerPCService.UpdateSlotItem(customerId, customPcId, componentType, productId);
        }
        public async Task RemoveSlotItem(int customerId, int customPcId, string componentType)
        {
            await _customerPCService.RemoveSlotItem(customerId, customPcId, componentType);
        }

        // Thêm cấu hình hiện tại vào giỏ hàng
        public async Task AddToCartFromSlot(int customerId, int slotId)
        {
            await _customerPCService.AddToCartFromSlot(customerId, slotId);
        }
    }
}
