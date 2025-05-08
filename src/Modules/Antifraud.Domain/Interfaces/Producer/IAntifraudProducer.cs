using Common.DTOs;
using Common.Result;

namespace Antifraud.Domain.Interfaces.Producer;

public interface IAntifraudProducer
{
    public Task<Result<bool>> ProduceAsync(TransactionProcessedStatusDTO transactionProcessedStatus);
}
