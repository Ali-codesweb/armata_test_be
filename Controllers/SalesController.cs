using armada_test.Dto;
using armada_test.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace armada_test.Controllers;

[ApiController()]
[Route("api/sales")]
public class SalesController(ISalesService salesService) : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> CreateSale([FromBody] List<Sales.SaleItemCreateDto> saleItemCreateDto)
    {
        var result = await salesService.CreateSale(saleItemCreateDto);
        return Ok(result);
    }
}