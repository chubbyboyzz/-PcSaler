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
   
}
