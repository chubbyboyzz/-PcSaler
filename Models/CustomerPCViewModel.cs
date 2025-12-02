

namespace PcSaler.Models
{
    public class CustomerPCViewModel
    {
        // Thông tin chung (Lấy từ bảng PCBuild)
        public int PCBuildID { get; set; }
        public string PCBuildName { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }

        // Danh sách linh kiện bên trong (Lấy từ bảng PCBuildDetails JOIN Products)
        public List<PCComponentViewModel> Components { get; set; }
    }

}
