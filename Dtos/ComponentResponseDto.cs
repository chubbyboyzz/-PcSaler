namespace PcSaler.Dtos
{
    public class ComponentResponseDto
    {
        public required List<ProductDto> Products { get; set; }
        public required List<string> AvailableBrands { get; set; }
        public required List<Models.PriceRangeViewModel> AvailablePriceRanges { get; set; }
        public required PaginationDto Pagination { get; set; }
    }
}
