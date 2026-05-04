using System.ComponentModel.DataAnnotations;

namespace armada_test.Models;

public class Payment
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public decimal Amount { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Success, Failed

    public string? TransactionId { get; set; }

    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}