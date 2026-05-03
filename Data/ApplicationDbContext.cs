using armada_test.Models;
using Microsoft.EntityFrameworkCore;

namespace armada_test.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Inventory.Product> products { get; set; }
    public DbSet<Inventory.StockLedgeEntry> stockLedger { get; set; }
    
}