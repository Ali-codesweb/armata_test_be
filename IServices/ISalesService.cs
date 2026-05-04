using armada_test.Dto;
using armada_test.Models;

namespace armada_test.IServices;

public interface ISalesService
{
    Task<ApiResponse<SaleCreateResponseDto>> CreateSale(List<Sales.SaleItemCreateDto> itemsDto);
    Task<ApiResponse<SaleDetailDto?>> GetSaleDetail(int saleId);
    Task<ApiResponse<string>> ReturnSale(int saleId);
}