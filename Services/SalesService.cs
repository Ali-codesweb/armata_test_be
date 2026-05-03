using armada_test.Data;
using armada_test.Dto;
using armada_test.IServices;
using armada_test.Models;
using Microsoft.EntityFrameworkCore;
using Inventory = armada_test.Dto.Inventory;
using ModelSchema = armada_test.Models.Inventory;


namespace armada_test.Services;

public class SalesService(ApplicationDbContext dbContext, IInventoryService inventoryService) : ISalesService
{
    public async Task<SaleCreateResponseDto> CreateSale(List<Sales.SaleItemCreateDto> itemsDto)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Validation logic
                var productIds = itemsDto.Select(x => x.Sku).ToList();
                var products = await dbContext.products
                    .Where(p => productIds.Contains(p.Sku))
                    .ToDictionaryAsync(p => p.Sku);

                foreach (var item in itemsDto)
                {
                    if (!products.ContainsKey(item.Sku))
                        return new SaleCreateResponseDto("Cannot find product", null);
                }

                var sale = new Sale
                {
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<SaleItem>()
                };

                // Action
                decimal totalPrice = 0;
                foreach (var item in itemsDto)
                {
                    var product = products[item.Sku];
                    inventoryService.ApplyStockChange(
                        product,
                        item.Quantity,
                        "DEDUCT",
                        "Sale"
                    );

                    var saleItem = new SaleItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    totalPrice += product.Price * item.Quantity;

                    sale.Items.Add(saleItem);
                }

                sale.TotalPrice = totalPrice;

                dbContext.sales.Add(sale);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SaleCreateResponseDto("Sale Created Successfully", sale.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}