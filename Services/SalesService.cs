using armada_test.Data;
using armada_test.Dto;
using armada_test.IServices;
using armada_test.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace armada_test.Services;

public class SalesService(
    ApplicationDbContext dbContext,
    IInventoryService inventoryService,
    IWebHostEnvironment webHostEnvironment,
    IHttpContextAccessor httpContextAccessor) : ISalesService
{
    public async Task<SaleCreateResponseDto> CreateSale(List<Sales.SaleItemCreateDto> itemsDto)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        int saleId;

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

                saleId = sale.Id;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }

            var fullSale = await dbContext.sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            var pdfUrl = GeneratePDF(fullSale!);

            return new SaleCreateResponseDto("Sale Created Successfully", saleId, pdfUrl);
        });
    }

    public async Task<SaleDetailDto?> GetSaleDetail(int saleId)
    {
        var sale = await dbContext.sales
            .Include(s => s.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null)
            return null;

        return new SaleDetailDto(
            sale.Id,
            sale.CreatedAt,
            sale.TotalPrice,
            sale.Items.Select(i => new SaleItemDetailDto(
                i.Product.Name,
                i.Quantity,
                i.UnitPrice
            )).ToList(),
            sale.IsReturned
        );
    }

    public async Task<string> ReturnSale(int saleId)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var sale = await dbContext.sales
                    .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(x => x.Id == saleId);

                if (sale == null)
                    return "Sale not found";

                if (sale.IsReturned)
                    return "Sale already returned";

                sale.IsReturned = true;

                foreach (var item in sale.Items)
                {
                    inventoryService.ApplyStockChange(
                        item.Product,
                        item.Quantity,
                        "ADD",
                        $"Return Item of Sale {sale.Id}"
                    );
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Sale returned successfully";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }


    private string GeneratePDF(Sale sale)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fileName = $"sale_{sale.Id}.pdf";
        var relativePath = Path.Combine("Assets", "SalesInvoice", fileName);
        var absolutePath = Path.Combine(webHostEnvironment.WebRootPath, relativePath);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.ContinuousSize(250f, Unit.Point); // Use ContinuousSize for receipts
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.CourierNew));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Text("---------------------------").AlignCenter();
                    col.Item().PaddingVertical(5).Text("MY STORE").FontSize(14).SemiBold().AlignCenter();
                    col.Item().Text("---------------------------").AlignCenter();

                    // Invoice Info
                    col.Item().PaddingTop(5).Text($"Invoice No: INV-{1000 + sale.Id}");
                    col.Item().Text($"Date: {sale.CreatedAt:dd-MM-yyyy}");

                    // Table Header
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem(3).Text("Item");
                        row.RelativeItem(1).Text("Qty").AlignCenter();
                        row.RelativeItem(2).Text("Price").AlignRight();
                    });
                    col.Item().Text("---------------------------").AlignCenter();

                    // Items
                    foreach (var item in sale.Items)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(3).Text(item.Product.Name);
                            row.RelativeItem(1).Text(item.Quantity.ToString()).AlignCenter();
                            row.RelativeItem(2).Text(item.UnitPrice.ToString("N3")).AlignRight();
                        });
                    }

                    col.Item().Text("---------------------------").AlignCenter();

                    // Total
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total:").SemiBold();
                        row.RelativeItem(3).Text(sale.TotalPrice.ToString("N3")).SemiBold().AlignRight();
                    });

                    col.Item().Text("---------------------------").AlignCenter();

                    // Payment
                    col.Item().Text("Payment: CARD");
                    col.Item().Text("---------------------------").AlignCenter();

                    // Footer
                    col.Item().PaddingTop(5).Text("Thank You!").AlignCenter();
                });
            });
        }).GeneratePdf(absolutePath);

        // Build absolute URL
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return relativePath.Replace("\\", "/");

        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        return $"{baseUrl}/{relativePath.Replace("\\", "/")}";
    }
}