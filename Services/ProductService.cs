using armada_test.Data;
using armada_test.IServices;
using armada_test.Models;
using Microsoft.EntityFrameworkCore;

namespace armada_test.Services;

public class ProductService(ApplicationDbContext dbContext) : IProductService
{
    public async Task<string> AddProduct(Dto.Inventory.ProductDto productDto)
    {
        var newProduct = new Inventory.Product
        {
            Name = productDto.Name,
            Price = productDto.Price,
            CurrentStock = productDto.CurrentStock,
            Color = productDto.Color,
            Size = productDto.Size,
            Sku = await GenerateSkuAsync("AMD", productDto.Color, productDto.Size)
        };

        dbContext.products.Add(newProduct);
        await dbContext.SaveChangesAsync();
        return "Product Created Successfully";
    }

    public async Task<List<Inventory.Product>> GetProducts()
    {
        var products = await dbContext.products.ToListAsync();
        return products;
    }


    private async Task<string> GenerateSkuAsync(string prefix, string color, string size)
    {
        var lastSku = await dbContext.products
            .Where(p => p.Sku.StartsWith(prefix))
            .OrderByDescending(p => p.Sku)
            .Select(p => p.Sku)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastSku != null)
        {
            var parts = lastSku.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        string numberPart = nextNumber.ToString("D5"); // 25001 format
        return $"{prefix}-{numberPart}-{color}-{size}";
    }
}