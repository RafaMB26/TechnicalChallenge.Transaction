using Transaction.Domain.DTOs;
using Transaction.Domain.Result;

namespace Transaction.Domain.Interfaces.Services;

public interface ITransactionService
{
    public Task<Result<TransactionDTO>> SendTransactionAsync(CreateTransactionDTO transaction);
}
