using Common.DTOs;
using Common.Result;
using Transaction.Domain.DTOs;


namespace Transaction.Domain.Interfaces.Services;

public interface ITransactionService
{
    public Task<Result<TransactionDTO>> GetTransactionByExternalIdAsync(Guid externalId);
    public Task<Result<TransactionDTO>> SendTransactionAsync(CreateTransactionDTO transaction);
}
