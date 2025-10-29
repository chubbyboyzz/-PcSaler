namespace PcSaler.DBcontext.Entites
{
    public class PCBuild
    {
        public int PCBuildID { get; set; }
        public string PCBuildName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal TotalPrice { get; set; } = 0m;
        public string? ImageURL { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<PCBuildDetail>? Details { get; set; }
    }
}
