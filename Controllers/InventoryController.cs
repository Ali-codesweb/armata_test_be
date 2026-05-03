using armada_test.Data;
using armada_test.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace armada_test.Controllers;

[ApiController]
public class InventoryController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("api/[controller]/[action]")]
    public async Task<IActionResult> CreateStock([FromBody] Inventory.ProductDto productDto)
    {
        if (await dbContext.products.AnyAsync(x => x.Sku == productDto.Sku))
        {
            return BadRequest("Kindly Provide Unique SKU");
        }

        return Ok();
    }
    
}