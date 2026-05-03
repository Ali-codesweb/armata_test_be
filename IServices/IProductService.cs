using armada_test.Data;
using armada_test.Models;

namespace armada_test.IServices;

public interface IProductService
{
    Task<string> AddProduct(Dto.Inventory.ProductDto productDto);
    Task<List<Inventory.Product>> GetProducts();
}