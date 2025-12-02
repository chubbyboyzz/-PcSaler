using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Categories>> GetAllCategories();
        Task<List<CategoryViewModel>> GetCategoryProducts(int? categoryId, string? query);
    }
}
