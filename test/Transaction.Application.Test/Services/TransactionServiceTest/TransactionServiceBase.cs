using Common.DTOs;
using Common.Enums;
using Common.Result;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Transaction.Application.Services;
using Transaction.Domain.DTOs;
using Transaction.Domain.Entities;
using Transaction.Domain.Interfaces.Producers;
using Transaction.Domain.Interfaces.Repositories;

namespace Transaction.Application.Test.Services.TransactionServiceTest
{
    public class TransactionServiceBase
    {
        protected ITransactionRepository _transactionRepositoryMock;
        protected ITransactionStatusRepository _transactionStatusRepositoryMock;
        protected ITransactionProducer _transactionProducerMock;
        protected ILogger<TransactionService> _loggerMock;
        protected TransactionService _transactionService;
        protected CreateTransactionDTO _transactionToCreate;
        protected TransactionProcessedStatusDTO _transactionToUpdate;

        [SetUp]
        public void Setup()
        {
            _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
            _transactionStatusRepositoryMock = Substitute.For<ITransactionStatusRepository>();
            _transactionProducerMock = Substitute.For<ITransactionProducer>();
            _loggerMock = Substitute.For<ILogger<TransactionService>>();

            _transactionService = new TransactionService(
                _transactionRepositoryMock,
                _transactionStatusRepositoryMock,
                _transactionProducerMock,
                _loggerMock);

            _transactionToCreate = new CreateTransactionDTO()
            {
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = 500
            };

            _transactionToUpdate = new TransactionProcessedStatusDTO()
            {
                IsCorrect = true,
                RejectedReason = String.Empty,
                TransactionExternalId = Guid.NewGuid()
            };

            var transactionEntity = new TransactionEntity()
            {
                Id = 1,
                TransactionExternalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = 200,
                Status = new TransactionStatusEntity()
                {
                    Id = 1,
                    Description = "Pending",
                },
                StatusId = 1,

            };

            var transactionResult = new Result<TransactionEntity>(transactionEntity);

            _transactionRepositoryMock.GetTransactionByPublicIdAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(transactionResult));

            var pendingTransactionTypeEntity = new TransactionStatusEntity()
            {
                Id = 1,
                Description = "Pending"
            };

            var transactionTypeResult = new Result<TransactionStatusEntity>(pendingTransactionTypeEntity);

            _transactionStatusRepositoryMock.GetTransactionTypeByName(Arg.Any<TransactionStatusEnum>())
                .Returns(Task.FromResult(transactionTypeResult));

            var createdTransactionResult = new Result<TransactionEntity>(transactionEntity);
            _transactionRepositoryMock.CreateAsync(Arg.Any<TransactionEntity>())
                .Returns(Task.FromResult(createdTransactionResult));

            _transactionProducerMock.ProduceAsync(Arg.Any<TransactionDTO>())
                .Returns(Task.FromResult(new Result<bool>(true)));

            _transactionRepositoryMock.UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>())
                .Returns(Task.FromResult(new Result<TransactionEntity>(transactionEntity)));

        }
    }
}
