using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel>? GetProductDetails(int id);
        Task<List<ProductListViewModel>> GetProductsByCategory(int? categoryId);
        Task<List<ProductListViewModel>> GetProductQuery(string? query);
        Task<string?> GetCategoryType(int categoryId);
        Task<ProductSearchResultViewModel> SearchProductsAsync(ProductSearchInputViewModel input);
    }
}
