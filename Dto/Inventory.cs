using System.ComponentModel.DataAnnotations;

namespace armada_test.Dto;

public class Inventory
{
    public class ProductDto
    {
        [Required]
        [MaxLength(100)]
        public string Sku { get; set; } = string.Empty; // SKU unique (e.g., 105105-25001-RED-SML)

        [Required] public string Name { get; set; } = string.Empty;
        [Required] [MaxLength(10)] public string Color { get; set; } = string.Empty;
        [Required] [MaxLength(5)] public string Size { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int CurrentStock { get; set; }
    }

    public class ProductStockUpdateDto
    {
        public string sku { get; set; }
        public int count { get; set; }
        public string reason { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
    }
}