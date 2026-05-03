using armada_test.Models;

namespace armada_test.Dto;

public class Sales
{
    public class SaleItemCreateDto
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}

public record SaleCreateResponseDto(
    string Message,
    int? saleId
);