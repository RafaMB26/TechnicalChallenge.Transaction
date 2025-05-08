using Common.DTOs;
using Common.Result;

namespace Antifraud.Domain.Interfaces.Services;

public interface IAntifraudService
{
    public Task<Result<TransactionProcessedStatusDTO>> IsTransactionCorrectAsync(TransactionDTO transaction);
}
