using armada_test.Dto;
using ModelSchema = armada_test.Models.Inventory;
using StockDto = armada_test.Dto.Inventory;

namespace armada_test.IServices;

public interface IInventoryService
{
    Task<ApiResponse<string>> AddProduct(StockDto.ProductDto productDto);
    Task<ApiResponse<List<ModelSchema.Product>>> GetProducts();
    Task<ApiResponse<ModelSchema.Product?>> GetStockBySku(string sku);
    Task<ApiResponse<string>> UpdateStock(StockDto.ProductStockUpdateDto payload);

    void ApplyStockChange(
        ModelSchema.Product product,
        int count,
        string action,
        string reason);
}