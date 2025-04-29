using Antifraud.Application.Services;
using Antifraud.Domain.Interfaces.Producer;
using Antifraud.Domain.Interfaces.Repositories;
using Common.DTOs;
using Common.Result;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Antifraud.Application.Test.Services.AntifraudServiceTest;
public class AntifraudServiceTestBase
{
    protected ILogger<AntifraudService> _loggerMock;
    protected ITransactionRepository _transactionRepositoryMock;
    protected IAntifraudProducer _antifraudProducerMock;
    protected TransactionDTO _transactionToValidate;
    protected AntifraudService _antifraudService;

    [SetUp]
    public void Setup()
    {
        _loggerMock = Substitute.For<ILogger<AntifraudService>>();
        _transactionRepositoryMock = Substitute.For<ITransactionRepository>();
        _antifraudProducerMock = Substitute.For<IAntifraudProducer>();

        _antifraudService = new AntifraudService(_transactionRepositoryMock, _antifraudProducerMock, _loggerMock);

        _transactionToValidate = new TransactionDTO()
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 200,
            Status = 1,
        };

        var sumOfTheDayResult = new Result<decimal>(200);

        _transactionRepositoryMock.GetSumOfTransactionsAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(sumOfTheDayResult));

        _transactionRepositoryMock.AddTransactionValueAsync(Arg.Any<Guid>(), 200)
            .Returns(Task.FromResult(new Result<bool>(true)));

        _antifraudProducerMock.ProduceAsync(Arg.Any<TransactionProcessedStatusDTO>())
            .Returns(Task.FromResult(new Result<bool>(true)));
    }
}