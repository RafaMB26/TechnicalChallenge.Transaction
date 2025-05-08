using Common.Result;

namespace Antifraud.Domain.Interfaces.Repositories;

public interface ITransactionRepository
{
    public Task<Result<decimal>> GetSumOfTransactionsAsync(Guid source);
    public Task<Result<bool>> AddTransactionValueAsync(Guid source, decimal amount);
}