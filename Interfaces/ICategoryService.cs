using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Categories>> GetAllCategories();
        Task<List<CategoryProductViewModel>> GetCategoryProducts(int? categoryId, string? query);
    }
}
