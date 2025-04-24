using Microsoft.Extensions.Logging;
using Transaction.Domain.Entities;
using Transaction.Domain.Interfaces.Repositories;

namespace Transaction.Infrastructure.Repositories;

public class TransactionRepository : Repository<TransactionEntity>, ITransactionRepository
{
    public TransactionRepository(TransactionDbContext dbContext, ILogger<Repository<TransactionEntity>> logger) : base(dbContext, logger)
    {
    }
}