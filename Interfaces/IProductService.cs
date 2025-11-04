using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface IProductService
    {
        List<CategoryProductViewModel> GetCategoryProducts(int? categoryId, string? query);
        ProductListViewModel? GetProductDetails(int id);
        List<Category> GetAllCategories();


    }
}
