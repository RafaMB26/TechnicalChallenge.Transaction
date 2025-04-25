using Common.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Transaction.Domain.Entities;
using Transaction.Domain.Interfaces.Repositories;

namespace Transaction.Infrastructure.Repositories;

public class TransactionRepository : Repository<TransactionEntity>, ITransactionRepository
{
    private readonly TransactionDbContext _transactionDbContext;
    public TransactionRepository(TransactionDbContext dbContext, ILogger<Repository<TransactionEntity>> logger) : base(dbContext, logger)
    {
        _transactionDbContext = dbContext;
    }

    public async Task<Result<TransactionEntity>> GetTransactionByPublicIdAsync(Guid publicId)
    {
        var transaction = await (from t in _transactionDbContext.Transactions
                                 where t.TransactionExternalId == publicId
                                 select t).SingleOrDefaultAsync();

        return new Result<TransactionEntity>(transaction);
    }
}