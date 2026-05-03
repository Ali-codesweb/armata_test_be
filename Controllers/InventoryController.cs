using armada_test.IServices;
using Microsoft.AspNetCore.Mvc;
using StockDto = armada_test.Dto.Inventory;

namespace armada_test.Controllers;

[ApiController]
[Route("api")]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] StockDto.ProductDto productDto)
    {
        var result = await inventoryService.AddProduct(productDto);
        return Ok(result);
    }

    [HttpGet("stock/{sku}")]
    public async Task<IActionResult> GetStock(string sku)
    {
        var product = await inventoryService.GetStockBySku(sku);
        if (product == null) return NotFound("Product not found");

        return Ok(new { product.Sku, product.Name, product.CurrentStock });
    }

    [HttpPut("stock/update")]
    public async Task<IActionResult> UpdateStock([FromBody] StockDto.ProductStockUpdateDto payload)
    {
        var result = await inventoryService.UpdateStock(payload);
        return Ok(result);
    }

    [HttpGet("items")]
    public async Task<IActionResult> ListItems()
    {
        var products = await inventoryService.GetProducts();
        return Ok(products);
    }
}