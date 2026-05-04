using armada_test.Dto;
using armada_test.IServices;
using Microsoft.AspNetCore.Mvc;

namespace armada_test.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] PaymentDto.PayRequestDto request)
    {
        var result = await paymentService.Pay(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] PaymentDto.RefundRequestDto request)
    {
        var result = await paymentService.Refund(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}