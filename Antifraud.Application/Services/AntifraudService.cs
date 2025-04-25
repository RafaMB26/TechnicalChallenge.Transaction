using Antifraud.Domain.Interfaces.Repositories;
using Antifraud.Domain.Interfaces.Services;
using Common.DTOs;
using Common.Result;
using Microsoft.Extensions.Logging;

namespace Antifraud.Application.Services;

public class AntifraudService : IAntifraudService
{
    private readonly ILogger<AntifraudService> _logger;
    private readonly ITransactionRepository _transactionRepository;
    public AntifraudService(ITransactionRepository transactionRepository, ILogger<AntifraudService> logger)
    {
        _logger = logger;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<TransactionProcessedStatusDTO>> IsTransactionCorrectAsync(TransactionDTO transaction)
    {
        try
        {
            var resultTransaction = new TransactionProcessedStatusDTO();
            if (transaction.Value > 2000)
            {
                resultTransaction.IsCorrect = false;
                resultTransaction.RejectedReason = "Transaction amount is greater than 2000";
                return new Result<TransactionProcessedStatusDTO>(resultTransaction);
            }


            var todayTransactionSumResult = await _transactionRepository.GetSumOfTransactionsAsync(transaction.SourceAccountId);
            if (!todayTransactionSumResult.IsSuccess)
                return new Result<TransactionProcessedStatusDTO>(todayTransactionSumResult.Error);


            if (todayTransactionSumResult.Data > 20000)
            {
                resultTransaction.IsCorrect = false;
                resultTransaction.RejectedReason = "Accumulated per day is greater than 20000";
                return new Result<TransactionProcessedStatusDTO>(resultTransaction);
            }

            resultTransaction.IsCorrect = true;

            await _transactionRepository.AddTransactionValueAsync(transaction.SourceAccountId, transaction.Value);

            return new Result<TransactionProcessedStatusDTO>(resultTransaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof}: Transaction {transaction}", nameof(IsTransactionCorrectAsync), transaction);
            return new Result<TransactionProcessedStatusDTO>(new Error($"Unexpected error while trying to verify the transaction {transaction.TransactionExternalId}.", 503));
        }
        
    }
}
