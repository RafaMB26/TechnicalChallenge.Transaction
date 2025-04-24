using Transaction.Domain.Entities;
using Transaction.Domain.Enums;
using Transaction.Domain.Result;

namespace Transaction.Domain.Interfaces.Repositories;

public interface ITransactionStatusRepository:IRepository<TransactionStatusEntity>
{
    public Task<Result<TransactionStatusEntity>> GetTransactionTypeByName(TransactionStatusEnum name);
}
