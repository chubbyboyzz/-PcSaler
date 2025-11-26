using PcSaler.DBcontext.Entites;

namespace PcSaler.Models
{
    public class ProductViewModel
    {
        public List<ProductListViewModel> CPUs { get; set; } = new();
        public List<ProductListViewModel> Mainboards { get; set; } = new();
        public List<ProductListViewModel> RAMs { get; set; } = new();
        public List<ProductListViewModel> GPUs { get; set; } = new();
        public List<ProductListViewModel> PSUs { get; set; } = new();
        public List<ProductListViewModel> Cases { get; set; } = new();
    }
}
