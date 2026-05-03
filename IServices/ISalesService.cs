using armada_test.Dto;

namespace armada_test.IServices;

public interface ISalesService
{
    Task<string> CreateSale(List<Sales.SaleItemCreateDto> itemsDto);
}