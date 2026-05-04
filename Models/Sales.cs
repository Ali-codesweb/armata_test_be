namespace armada_test.Models;

public class Sale
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalPrice { get; set; }

    public List<SaleItem> Items { get; set; } = new();

    public bool IsReturned { get; set; }

    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
}

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public Inventory.Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}