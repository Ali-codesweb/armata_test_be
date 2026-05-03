using armada_test.Data;
using armada_test.Dto;
using armada_test.IServices;
using Microsoft.AspNetCore.Mvc;

namespace armada_test.Controllers;

[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpPost("api/[controller]/[action]")]
    public async Task<IActionResult> AddProduct([FromBody] Inventory.ProductDto productDto)
    {
        var message = await productService.AddProduct(productDto);
        return Ok(message);
    }

    [HttpGet("api/[controller]/[action]")]
    public async Task<IActionResult> ListProducts()
    {
        var products = await productService.GetProducts();
        return Ok(products);
    }
}