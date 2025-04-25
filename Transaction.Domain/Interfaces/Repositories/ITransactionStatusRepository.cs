using Common.Enums;
using Common.Interfaces;
using Common.Result;
using Transaction.Domain.Entities;

namespace Transaction.Domain.Interfaces.Repositories;

public interface ITransactionStatusRepository:IRepository<TransactionStatusEntity>
{
    public Task<Result<TransactionStatusEntity>> GetTransactionTypeByName(TransactionStatusEnum name);
}
