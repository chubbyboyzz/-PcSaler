using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel>? GetProductDetails(int id);
        Task<List<ProductListViewModel>> GetProductsByCategory(int? categoryId);
    }
}
