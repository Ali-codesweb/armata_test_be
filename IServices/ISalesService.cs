using armada_test.Dto;
using armada_test.Models;

namespace armada_test.IServices;

public interface ISalesService
{
    Task<SaleCreateResponseDto> CreateSale(List<Sales.SaleItemCreateDto> itemsDto);
}