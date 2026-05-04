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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSaleDetail(int id)
    {
        var result = await salesService.GetSaleDetail(id);
        return Ok(result);
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> ReturnSale(int id)
    {
        var result = await salesService.ReturnSale(id);
        return Ok(result);
    }
}