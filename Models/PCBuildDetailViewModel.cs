

namespace PcSaler.Models
{
    public class PCBuildDetailViewModel
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

    public class BuildCategoryDto
    {
        public string Id { get; set; }   // ComponentType (VD: CPU)
        public string Name { get; set; } // Description (VD: Vi xử lý)
    }
}
