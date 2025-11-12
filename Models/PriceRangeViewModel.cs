namespace PcSaler.Models
{
    public class PriceRangeViewModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }
}
