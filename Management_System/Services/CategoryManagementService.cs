using PcSaler.DBcontext.Entites;
using PcSaler.Management_System.Interfaces;

namespace PcSaler.Management_System.Services
{
    public class CategoryManagementService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryManagementService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Categories>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Categories?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Categories> CreateCategoryAsync(Categories category)
        {
            return await _categoryRepository.CreateAsync(category);
        }

        public async Task<Categories> UpdateCategoryAsync(Categories category)
        {
            return await _categoryRepository.UpdateAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }

        public async Task<int> GetTotalCategoriesAsync()
        {
            return await _categoryRepository.GetTotalCountAsync();
        }
    }
}
