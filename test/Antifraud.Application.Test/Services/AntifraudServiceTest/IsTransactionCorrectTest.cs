using Common.DTOs;
using Common.Result;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Antifraud.Application.Test.Services.AntifraudServiceTest;

[TestFixture]
public class IsTransactionCorrectTest : AntifraudServiceTestBase
{
    [Test]
    public async Task IsTransactionCorrect_TransactionIsCorrect_CallProducer()
    {
        var result = await _antifraudService.IsTransactionCorrectAsync(_transactionToValidate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result.Data.IsCorrect);

        await _antifraudProducerMock.Received(1).ProduceAsync(Arg.Any<TransactionProcessedStatusDTO>());
    }

    [Test]
    public async Task IsTransactionCorrect_TransactionRepositoryThrowsError_DoNotCallProducer()
    {
        var errorMessage = "Error while trying to get the sum of the customer";
        _transactionRepositoryMock.GetSumOfTransactionsAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(new Result<decimal>(new Error(errorMessage, 503))));

        var result = await _antifraudService.IsTransactionCorrectAsync(_transactionToValidate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo(errorMessage));
        Assert.That(result.Error.Code, Is.EqualTo(503));

        await _antifraudProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionProcessedStatusDTO>());
    }


    [Test]
    public async Task IsTransactionCorrect_TransactionValueIsGraterThanTwoThousand_TransactionIsNotCorrect()
    {
        _transactionToValidate.Value = 2001;
        var result = await _antifraudService.IsTransactionCorrectAsync(_transactionToValidate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsFalse(result.Data.IsCorrect);

        await _antifraudProducerMock.Received(1).ProduceAsync(Arg.Any<TransactionProcessedStatusDTO>());
    }

    [Test]
    public async Task IsTransactionCorrect_SumOfTheDayIsGraterThanTwentyThousand_TransactionIsNotCorrect()
    {
        _transactionRepositoryMock.GetSumOfTransactionsAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(new Result<decimal>(20001)));
        var result = await _antifraudService.IsTransactionCorrectAsync(_transactionToValidate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsFalse(result.Data.IsCorrect);

        await _antifraudProducerMock.Received(1).ProduceAsync(Arg.Any<TransactionProcessedStatusDTO>());
    }

    [Test]
    public async Task IsTransactionCorrect_TransactionRepositoryThrowsException_DoNotCallProducer()
    {

        _transactionRepositoryMock.GetSumOfTransactionsAsync(Arg.Any<Guid>())
            .ThrowsAsync(new Exception("Exception"));

        var result = await _antifraudService.IsTransactionCorrectAsync(_transactionToValidate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.IsTrue(result.Error.Message.StartsWith("Unexpected error while trying to verify the transaction"));
        Assert.That(result.Error.Code, Is.EqualTo(503));

        await _antifraudProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionProcessedStatusDTO>());
    }
}
