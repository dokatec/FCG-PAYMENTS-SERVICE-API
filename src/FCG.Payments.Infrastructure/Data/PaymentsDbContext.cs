using FCG.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Infrastructure.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
         .Property(t => t.Status)
         .HasConversion<string>();

        base.OnModelCreating(modelBuilder);

    }
}