using Common.Interfaces;
using Common.Result;
using Transaction.Domain.Entities;

namespace Transaction.Domain.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<TransactionEntity>
{
    public Task<Result<TransactionEntity>> GetTransactionByPublicIdAsync(Guid publicId);
}
