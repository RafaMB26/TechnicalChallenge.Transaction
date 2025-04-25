using Common.DTOs;
using Common.Result;

namespace Transaction.Domain.Interfaces.Producers;

public interface ITransactionProducer
{
    public Task<Result<bool>> ProduceAsync(TransactionDTO transactionEvent);
}
