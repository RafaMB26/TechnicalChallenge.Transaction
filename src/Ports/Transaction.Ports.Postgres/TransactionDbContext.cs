using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Transaction.Domain.Entities;
using Transaction.Ports.Postgres.EntityMapping;

namespace Transaction.Ports.Postgres;

public class TransactionDbContext:DbContext
{
    private ILogger<TransactionDbContext> _logger;
    public TransactionDbContext(DbContextOptions options, ILogger<TransactionDbContext> logger):base(options)
    {
        _logger = logger;
    }

    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TransactionStatusEntity> TransactionStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        try
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new TransactionStatusEntityMapping());
            modelBuilder.ApplyConfiguration(new TransactionEntityMapping());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on model creation");
            throw;
        }
        
    }
}
