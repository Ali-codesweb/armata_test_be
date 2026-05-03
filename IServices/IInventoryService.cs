using ModelSchema = armada_test.Models.Inventory;
using StockDto = armada_test.Dto.Inventory;

namespace armada_test.IServices;

public interface IInventoryService
{
    Task<string> AddProduct(StockDto.ProductDto productDto);
    Task<List<ModelSchema.Product>> GetProducts();
    Task<ModelSchema.Product?> GetStockBySku(string sku);
}