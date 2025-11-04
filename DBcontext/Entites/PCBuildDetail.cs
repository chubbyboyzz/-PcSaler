using System.ComponentModel.DataAnnotations;

namespace PcSaler.DBcontext.Entites
{
    public class PCBuildDetail
    {
        [Key]
        public int PCBuildDetailID { get; set; }
        public int PCBuildID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; } = 1;

        // Navigation
        public PCBuild PCBuild { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
