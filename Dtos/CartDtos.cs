
namespace PcSaler.Models.DTOs
{
    public class UpdateCartDto
    {
        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public int Quantity { get; set; }
    }
    public class RemoveCartDto
    {
        public int ItemID { get; set; }
        public string ItemType { get; set; }
    }
}