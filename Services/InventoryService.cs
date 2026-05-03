using armada_test.Data;
using armada_test.IServices;
using Microsoft.EntityFrameworkCore;
using ModelSchema = armada_test.Models.Inventory;
using StockDto = armada_test.Dto.Inventory;

namespace armada_test.Services;

public class InventoryService(ApplicationDbContext dbContext) : IInventoryService
{
    public async Task<string> AddProduct(StockDto.ProductDto productDto)
    {
        // 1. Check for SKU uniqueness (as per PDF rules)
        if (await dbContext.products.AnyAsync(x => x.Sku == productDto.Sku))
        {
            throw new Exception("SKU must be unique.");
        }

        // Use the execution strategy for retriable transactions
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var newProduct = new ModelSchema.Product
                {
                    Sku = productDto.Sku,
                    Name = productDto.Name,
                    Price = productDto.Price,
                    CurrentStock = productDto.CurrentStock,
                    Color = productDto.Color,
                    Size = productDto.Size
                };

                dbContext.products.Add(newProduct);
                await dbContext.SaveChangesAsync();

                // 2. Maintain stock ledger (Create initial entry)
                var ledgerEntry = new ModelSchema.StockLedgeEntry
                {
                    ItemId = newProduct.Id,
                    QuantityChange = productDto.CurrentStock,
                    Reason = "Initial Stock Registration",
                    Timestamp = DateTime.UtcNow
                };

                dbContext.stockLedger.Add(ledgerEntry);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return "Product and Initial Stock Ledger created successfully";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<List<ModelSchema.Product>> GetProducts()
    {
        return await dbContext.products.ToListAsync();
    }

    public async Task<ModelSchema.Product?> GetStockBySku(string sku)
    {
        return await dbContext.products.FirstOrDefaultAsync(p => p.Sku == sku);
    }
}