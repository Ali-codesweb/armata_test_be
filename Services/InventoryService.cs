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
        if (await dbContext.products.AnyAsync(x => x.Sku == productDto.Sku))
        {
            throw new Exception("SKU must be unique.");
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        // Atomic Transaction
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                if (productDto.CurrentStock < 1)
                {
                    return "Product Stock must not be less than 1";
                }

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

                // Initial entry
                var ledgerEntry = new ModelSchema.StockLedgeEntry
                {
                    ItemId = newProduct.Id,
                    QuantityChange = productDto.CurrentStock,
                    Reason = "Initial Stock Registration",
                    Timestamp = DateTime.UtcNow,
                    Item = newProduct
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

    public async Task<string> UpdateStock(StockDto.ProductStockUpdateDto payload)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var product = await dbContext.products.FirstOrDefaultAsync(x => x.Sku == payload.sku);
                if (product == null) return "Product Not found";
                ApplyStockChange(product, payload.count, payload.action, payload.reason);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Product Stock Updated Successfully";
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public void ApplyStockChange(
        ModelSchema.Product product,
        int count,
        string action,
        string reason)
    {
        switch (action)
        {
            case "ADD":
                product.CurrentStock += count;
                break;

            case "DEDUCT" when product.CurrentStock >= count:
                product.CurrentStock -= count;
                break;

            case "DEDUCT":
                throw new Exception("Insufficient stock");

            default:
                throw new Exception("Invalid action");
        }

        dbContext.stockLedger.Add(new ModelSchema.StockLedgeEntry
        {
            Item = product,
            ItemId = product.Id,
            QuantityChange = action == "DEDUCT" ? -count : count,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        });
    }
}