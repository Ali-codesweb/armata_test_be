using armada_test.Data;
using armada_test.Dto;
using armada_test.IServices;
using Microsoft.EntityFrameworkCore;
using ModelSchema = armada_test.Models.Inventory;
using StockDto = armada_test.Dto.Inventory;

namespace armada_test.Services;

public class InventoryService(ApplicationDbContext dbContext) : IInventoryService
{
    public async Task<ApiResponse<string>> AddProduct(StockDto.ProductDto productDto)
    {
        if (await dbContext.products.AnyAsync(x => x.Sku == productDto.Sku))
        {
            return new ApiResponse<string>(false, "SKU must be unique.");
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
                    return new ApiResponse<string>(false, "Product Stock must not be less than 1");
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
                return new ApiResponse<string>(true, "Product and Initial Stock Ledger created successfully", "Success");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>(false, $"Error: {ex.Message}");
            }
        });
    }

    public async Task<ApiResponse<List<ModelSchema.Product>>> GetProducts()
    {
        var products = await dbContext.products.ToListAsync();
        return new ApiResponse<List<ModelSchema.Product>>(true, "Products retrieved successfully", products);
    }

    public async Task<ApiResponse<ModelSchema.Product?>> GetStockBySku(string sku)
    {
        var product = await dbContext.products.FirstOrDefaultAsync(p => p.Sku == sku);
        if (product == null)
        {
            return new ApiResponse<ModelSchema.Product?>(false, "Product not found");
        }
        return new ApiResponse<ModelSchema.Product?>(true, "Product retrieved successfully", product);
    }

    public async Task<ApiResponse<string>> UpdateStock(StockDto.ProductStockUpdateDto payload)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var product = await dbContext.products.FirstOrDefaultAsync(x => x.Sku == payload.sku);
                if (product == null) return new ApiResponse<string>(false, "Product Not found");
                
                ApplyStockChange(product, payload.count, payload.action, payload.reason);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<string>(true, "Product Stock Updated Successfully", "Success");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>(false, $"Error: {ex.Message}");
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