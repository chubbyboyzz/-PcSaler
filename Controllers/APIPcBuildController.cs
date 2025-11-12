using Microsoft.AspNetCore.Mvc;
using PcSaler.DBcontext;

[ApiController]
[Route("api/[controller]")]
public class APIPcBuildController : ControllerBase
{
    private readonly PCShopContext _context;

    public APIPcBuildController(PCShopContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult GetProducts([FromQuery] string category)
    {
        var products = _context.Products
            .Where(p => p.Category.ComponentType == category)
            .Select(p => new {
                productID = p.ProductID,
                productName = p.ProductName,
                brand = p.Brand,
                price = p.Price,
                imageURL = p.ImageURL
            })
            .ToList();

        return Ok(products);
    }
}