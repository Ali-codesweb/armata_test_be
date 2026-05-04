using armada_test.Dto;

namespace armada_test.IServices;

public interface IPaymentService
{
    Task<ApiResponse<PaymentDto.PaymentResponseDto>> Pay(PaymentDto.PayRequestDto request);
    Task<ApiResponse<PaymentDto.PaymentResponseDto>> Refund(PaymentDto.RefundRequestDto request);
}