using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Services
{
    public class ProductManagementService
    {
        private readonly IProductRepository _productRepository;

        public ProductManagementService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.GetByCategoryAsync(categoryId);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            return await _productRepository.CreateAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _productRepository.ExistsAsync(id);
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _productRepository.GetTotalCountAsync();
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            return await _productRepository.GetTotalStockValueAsync();
        }
    }
}
