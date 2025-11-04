using PcSaler.DBcontext.Entites;
using PcSaler.Models;

namespace PcSaler.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel>? GetProductDetails(int id);
    }
}
