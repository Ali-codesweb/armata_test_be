namespace armada_test.Dto;

public class PaymentDto
{
    public record PayRequestDto(int SaleId);
    public record RefundRequestDto(int SaleId);
    
    public record PaymentResponseDto(
        bool Success,
        string Message,
        string? TransactionId = null
    );
}