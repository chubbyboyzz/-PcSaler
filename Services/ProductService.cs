using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
using PcSaler.Interfaces;
using PcSaler.Models;

public class ProductService 
{
    private readonly IProductService _productService;

    public ProductService(IProductService productService)
    {
        _productService = productService;
    }
    public async Task<ProductListViewModel>? GetProductDetails(int id)
    {
        return await _productService.GetProductDetails(id);
    }
    public async Task<List<ProductListViewModel>> GetProductsByCategory(int? categoryId)
    {
        return await _productService.GetProductsByCategory(categoryId);
    }
    public async Task<List<ProductListViewModel>> GetProductQuery(string? query)
    {
        return await _productService.GetProductQuery(query);
    }
    public async Task<string?> GetCategoryType(int categoryId)
    {
        return await _productService.GetCategoryType(categoryId);
    }
    public async Task<ProductSearchResultViewModel> SearchProductsAsync(ProductSearchInputViewModel input)
    {
        return await _productService.SearchProductsAsync(input);
    }
}
