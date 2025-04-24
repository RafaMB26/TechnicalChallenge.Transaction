using Microsoft.EntityFrameworkCore;
using Transaction.Domain.Entities;
using Transaction.Infrastructure.EntityMapping;

namespace Transaction.Infrastructure;

public class TransactionDbContext:DbContext
{
    public TransactionDbContext(DbContextOptions options):base(options)
    {
    }

    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TransactionStatusEntity> TransactionStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TransactionStatusEntityMapping());
        modelBuilder.ApplyConfiguration(new TransactionEntityMapping());
    }
}
