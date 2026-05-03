using System.ComponentModel.DataAnnotations;

namespace armada_test.Models;

public class Inventory
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sku { get; set; } = string.Empty; // SKU unique (e.g., 105105-25001-RED-SML)

        [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
        [Required] [MaxLength(10)] public string Color { get; set; } = string.Empty;
        [Required] [MaxLength(5)] public string Size { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int CurrentStock { get; set; }
    }

    public class StockLedgeEntry
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Product Item { get; set; } = null!;
    }
}