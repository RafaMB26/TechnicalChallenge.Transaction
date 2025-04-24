using Microsoft.Extensions.Logging;
using Transaction.Domain.DTOs;
using Transaction.Domain.Entities;
using Transaction.Domain.Enums;
using Transaction.Domain.Interfaces.Repositories;
using Transaction.Domain.Interfaces.Services;
using Transaction.Domain.Result;

namespace Transaction.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionStatusRepository _transactionStatusRepository;
    private readonly ILogger<TransactionService> _logger;
    public TransactionService(
        ITransactionRepository transactionRepository, 
        ITransactionStatusRepository transactionStatusRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _transactionStatusRepository = transactionStatusRepository;
        _logger = logger;
    }
    public async Task<Result<TransactionDTO>> SendTransactionAsync(CreateTransactionDTO transaction)
    {
        try
        {
            if (transaction.TargetAccountId == Guid.Empty)
                return new Result<TransactionDTO>(new Error("The target account is not valid.", 400));
            if (transaction.SourceAccountId == Guid.Empty)
                return new Result<TransactionDTO>(new Error("The source account is not valid.", 400));
            if (transaction.Value <= 0)
                return new Result<TransactionDTO>(new Error("The value can not be less or equal to zero.", 400));

            var pendingStatusResult = await _transactionStatusRepository.GetTransactionTypeByName(TransactionStatusEnum.Pending);
            if (!pendingStatusResult.IsSuccess)
                return new Result<TransactionDTO>(pendingStatusResult.Error);

            var newTransaction = new TransactionEntity()
            {
                TargetAccountId = transaction.TargetAccountId,
                SourceAccountId = transaction.SourceAccountId,
                StatusId = pendingStatusResult.Data.Id,
                Value = transaction.Value,
                TransferTypeId = transaction.TransferTypeId,
                CreatedAt = DateTime.UtcNow,
                TransactionExternalId = new Guid(),
            };

            var createTransactionResult = await _transactionRepository.CreateAsync(newTransaction);
            if (!createTransactionResult.IsSuccess)
                return new Result<TransactionDTO>(createTransactionResult.Error);

            var transactionData = createTransactionResult.Data;

            var createdTransaction = new TransactionDTO()
            {
                CreatedAt = transactionData.CreatedAt,
                SourceAccountId = transactionData.SourceAccountId,
                Status = transactionData.Status.Id,
                TargetAccountId = transactionData.TargetAccountId,
                TransactionExternalId = transactionData.TransactionExternalId,
                TransferTypeId = transactionData.TransferTypeId,
                Value = transactionData.Value
            };
            return new Result<TransactionDTO>(createdTransaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(SendTransactionAsync), ex.Message);
            return new Result<TransactionDTO>(new Error("Unexpected error trying to send the transaction", 503));
        }
    }
}
