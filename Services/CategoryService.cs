using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;

namespace PcSaler.Services
{
    public class CategoryService
    {
        private readonly ICategoryService _categoryService;

        public CategoryService(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<List<Category>> GetAllCategories()
        {
            return await _categoryService.GetAllCategories();
        }
        public async Task<List<CategoryProductViewModel>> GetCategoryProducts(int? categoryId, string? query)
        {
            return await _categoryService.GetCategoryProducts(categoryId, query);
        }

    }
}
