namespace PcSaler.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public string? Img { get; set; }
        public required string Brand { get; set; }
    }
}
