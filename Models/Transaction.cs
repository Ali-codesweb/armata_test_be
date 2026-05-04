namespace armada_test.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public Sale Sale { get; set; } = new();
    public bool IsReturned { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime Date { get; set; }
    
}