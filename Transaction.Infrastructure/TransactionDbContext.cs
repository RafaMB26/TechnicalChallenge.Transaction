using Microsoft.EntityFrameworkCore;
using Transaction.Domain.Entities;

namespace Transaction.Infrastructure;

public class TransactionDbContext:DbContext
{
    public TransactionDbContext(DbContextOptions options):base(options)
    {
    }

    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TransactionStatusEntity> TransactionStatuses { get; set; }
}
