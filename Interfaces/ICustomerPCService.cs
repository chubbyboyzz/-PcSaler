using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface ICustomerPCService
    {
        Task<List<PCBuildDetailViewModel>> GetUserSlots(int customerId);
        Task UpdateSlotItem(int customerId, int customPcId, string componentType, int productId);
        Task RemoveSlotItem(int customerId, int customPcId, string componentType);
        Task AddToCartFromSlot(int customerId, int slotId);
    }
}
