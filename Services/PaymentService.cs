using armada_test.Data;
using armada_test.Dto;
using armada_test.IServices;
using armada_test.Models;
using Microsoft.EntityFrameworkCore;

namespace armada_test.Services;

public class PaymentService(ApplicationDbContext dbContext, IInventoryService inventoryService) : IPaymentService
{
    private readonly Random _random = new();

    public async Task<ApiResponse<PaymentDto.PaymentResponseDto>> Pay(PaymentDto.PayRequestDto request)
    {
        var sale = await dbContext.sales.FirstOrDefaultAsync(s => s.Id == request.SaleId);
        if (sale == null) 
            return new ApiResponse<PaymentDto.PaymentResponseDto>(false, "Sale not found");
        
        if (sale.PaymentStatus == "Paid") 
            return new ApiResponse<PaymentDto.PaymentResponseDto>(false, "Sale already paid");

        // Simulate Bank Response (80% success rate)
        bool isSuccess = _random.Next(1, 101) <= 80;
        string transactionId = Guid.NewGuid().ToString();

        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = sale.TotalPrice,
            Status = isSuccess ? "Success" : "Failed",
            TransactionId = isSuccess ? transactionId : null,
            CreatedAt = DateTime.UtcNow
        };

        if (isSuccess)
        {
            sale.PaymentStatus = "Paid";
        }
        else
        {
            sale.PaymentStatus = "Failed";
        }

        dbContext.payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var response = isSuccess 
            ? new PaymentDto.PaymentResponseDto(true, "Payment successful", transactionId)
            : new PaymentDto.PaymentResponseDto(false, "Payment failed at Bank");

        return new ApiResponse<PaymentDto.PaymentResponseDto>(isSuccess, response.Message, response);
    }

    public async Task<ApiResponse<PaymentDto.PaymentResponseDto>> Refund(PaymentDto.RefundRequestDto request)
    {
        var sale = await dbContext.sales
            .Include(s => s.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId);

        if (sale == null) 
            return new ApiResponse<PaymentDto.PaymentResponseDto>(false, "Sale not found");
        
        if (sale.PaymentStatus != "Paid") 
            return new ApiResponse<PaymentDto.PaymentResponseDto>(false, "Only paid sales can be refunded");
        
        if (sale.IsReturned) 
            return new ApiResponse<PaymentDto.PaymentResponseDto>(false, "Sale already refunded/returned");

        // Simulate Bank Refund Response (Always succeed for mock)
        string transactionId = Guid.NewGuid().ToString();

        var payment = new Payment
        {
            SaleId = sale.Id,
            Amount = -sale.TotalPrice, // Negative for refund
            Status = "Success",
            TransactionId = transactionId,
            CreatedAt = DateTime.UtcNow
        };

        sale.PaymentStatus = "Refunded";
        sale.IsReturned = true;

        // Restore Stock
        foreach (var item in sale.Items)
        {
            inventoryService.ApplyStockChange(item.Product, item.Quantity, "ADD", "Refund");
        }

        dbContext.payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var response = new PaymentDto.PaymentResponseDto(true, "Refund processed successfully", transactionId);
        return new ApiResponse<PaymentDto.PaymentResponseDto>(true, response.Message, response);
    }
}