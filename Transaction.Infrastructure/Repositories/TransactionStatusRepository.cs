using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Transaction.Domain.Entities;
using Transaction.Domain.Enums;
using Transaction.Domain.Interfaces.Repositories;
using Transaction.Domain.Result;

namespace Transaction.Infrastructure.Repositories;

public class TransactionStatusRepository : Repository<TransactionStatusEntity>, ITransactionStatusRepository
{
    private readonly TransactionDbContext _dbContext;
    private readonly ILogger<TransactionStatusRepository> _logger;
    public TransactionStatusRepository(TransactionDbContext dbContext, ILogger<TransactionStatusRepository> logger) 
        : base(dbContext, logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Result<TransactionStatusEntity>> GetTransactionTypeByName(TransactionStatusEnum name)
    {
        try
        {
            var transactionStatus = await (from status in _dbContext.TransactionStatuses
                                           where status.Description == name.ToString()
                                           select status).SingleOrDefaultAsync();

            return new Result<TransactionStatusEntity>(transactionStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(GetTransactionTypeByName), ex.Message);
            return new Result<TransactionStatusEntity>(new Error("Error while trying to get the transaction status", 503));
        }
       
    }
}
