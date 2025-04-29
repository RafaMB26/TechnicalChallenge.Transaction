using Common.DTOs;
using Common.Enums;
using Common.Result;
using Microsoft.Extensions.Logging;
using Transaction.Domain.DTOs;
using Transaction.Domain.Entities;
using Transaction.Domain.Interfaces.Producers;
using Transaction.Domain.Interfaces.Repositories;
using Transaction.Domain.Interfaces.Services;

namespace Transaction.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionStatusRepository _transactionStatusRepository;
    private readonly ITransactionProducer _transactionProducer;
    private readonly ILogger<TransactionService> _logger;
    public TransactionService(
        ITransactionRepository transactionRepository, 
        ITransactionStatusRepository transactionStatusRepository,
        ITransactionProducer transactionProducer,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _transactionStatusRepository = transactionStatusRepository;
        _transactionProducer = transactionProducer;
        _logger = logger;
    }

    public async Task<Result<TransactionDTO>> GetTransactionByExternalIdAsync(Guid externalId)
    {
        try
        {
            var transactionResult = await _transactionRepository.GetTransactionByPublicIdAsync(externalId);
            if (!transactionResult.IsSuccess)
                return new Result<TransactionDTO>(transactionResult.Error);
            if (transactionResult.Data is null)
                return new Result<TransactionDTO>(new Error($"The transaction with id {externalId} does not exist.", 404));

            var transaction = transactionResult.Data;
            var transactionDto = new TransactionDTO()
            {
                CreatedAt = transaction.CreatedAt,
                SourceAccountId = transaction.SourceAccountId,
                Status = transaction.StatusId,
                TargetAccountId = transaction.TargetAccountId,
                TransactionExternalId = transaction.TransactionExternalId,
                TransferTypeId = transaction.TransferTypeId,
                Value = transaction.Value
            };
            return new Result<TransactionDTO>(transactionDto);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(GetTransactionByExternalIdAsync), ex.Message);
            return new Result<TransactionDTO>(new Error($"Unexpected error trying to get the transaction {externalId}", 503));
        }
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
                TransactionExternalId = Guid.NewGuid(),
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

            await _transactionProducer.ProduceAsync(createdTransaction);

            return new Result<TransactionDTO>(createdTransaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} : Error {ex}", nameof(SendTransactionAsync), ex.Message);
            return new Result<TransactionDTO>(new Error("Unexpected error trying to send the transaction", 503));
        }
    }

    public async Task<Result<TransactionDTO>> UpdateTransactionStatus(TransactionProcessedStatusDTO transactionStatus)
    {
        var currentTransactionResult = await _transactionRepository.GetTransactionByPublicIdAsync(transactionStatus.TransactionExternalId);
        if(!currentTransactionResult.IsSuccess)
        {
            return new Result<TransactionDTO>(currentTransactionResult.Error);
        }
        if(currentTransactionResult.Data is null)
        {
            return new Result<TransactionDTO>(new Error($"The transaction {transactionStatus.TransactionExternalId} does not exist.", 404));
        }

        var statusToUpdate = transactionStatus.IsCorrect ? TransactionStatusEnum.Approved : TransactionStatusEnum.Rejected;
        var currentTransaction = currentTransactionResult.Data!;
        var statusResult = await _transactionStatusRepository.GetTransactionTypeByName(statusToUpdate);

        if (!statusResult.IsSuccess)
            _logger.LogError("An error happened while trying to get the status Id {statusToUpdate}. Error : {error}", statusToUpdate, statusResult.Error);

        currentTransaction.Status = statusResult.IsSuccess ? statusResult.Data : currentTransaction.Status;
        var statusUpdateResult = await _transactionRepository.UpdateAsync(currentTransaction.Id, currentTransaction);
        if (!statusUpdateResult.IsSuccess)
            _logger.LogError("An error happened while trying to update the status of the transaction {transactionExternalId} to {statusToUpdate}", transactionStatus.TransactionExternalId, statusToUpdate);

        var updatedTransaction = new TransactionDTO()
        {
            CreatedAt = currentTransaction.CreatedAt,
            SourceAccountId = currentTransaction.SourceAccountId,
            Status = currentTransaction.Status.Id,
            TargetAccountId = currentTransaction.TargetAccountId,
            TransactionExternalId = currentTransaction.TransactionExternalId,
            TransferTypeId = currentTransaction.TransferTypeId,
            Value = currentTransaction.Value
        };
        return new Result<TransactionDTO>(updatedTransaction);
    }
}
