using Common.DTOs;
using Common.Enums;
using Common.Result;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Transaction.Domain.DTOs;
using Transaction.Domain.Entities;

namespace Transaction.Application.Test.Services.TransactionServiceTest;

[TestFixture]
public class SendTransactionTest : TransactionServiceBase
{
    [Test]
    public async Task SendTransactionAsync_ValuesAreCorrect_ProducerIsCalled()
    {
        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.That(result.Data.Value, Is.GreaterThan(0));
        await _transactionProducerMock.Received(1).ProduceAsync(Arg.Any<TransactionDTO>());
    }

    [Test]
    public async Task SendTransactionAsync_TargetAccountIsWrong_ProducerIsNotCalled()
    {
        _transactionToCreate.TargetAccountId = new Guid();

        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo("The target account is not valid."));
        Assert.That(result.Error.Code, Is.EqualTo(400));
        await _transactionRepositoryMock.DidNotReceive().CreateAsync(Arg.Any<TransactionEntity>());
        await _transactionProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionDTO>());
    }

    [Test]
    public async Task SendTransactionAsync_SourceAccountIsWrong_ProducerIsNotCalled()
    {
        _transactionToCreate.SourceAccountId = new Guid();
        
        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo("The source account is not valid."));
        Assert.That(result.Error.Code, Is.EqualTo(400));
        await _transactionRepositoryMock.DidNotReceive().CreateAsync(Arg.Any<TransactionEntity>());
        await _transactionProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionDTO>());
    }

    [Test]
    [TestCase(0)]
    [TestCase(-100)]
    public async Task SendTransactionAsync_ValueIsLessOrEqualToZero_ProducerIsNotCalled(decimal value)
    {
        _transactionToCreate.Value = value;

        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo("The value can not be less or equal to zero."));
        Assert.That(result.Error.Code, Is.EqualTo(400));
        await _transactionRepositoryMock.DidNotReceive().CreateAsync(Arg.Any<TransactionEntity>());
        await _transactionProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionDTO>());
    }

    [Test]
    public async Task SendTransactionAsync_TransactionIdDoesNotExist_ProducerIsNotCalled()
    {
        var errorMessage = "Not valid transaction";
        _transactionStatusRepositoryMock.GetTransactionTypeByName(Arg.Any<TransactionStatusEnum>())
            .Returns(Task.FromResult(new Result<TransactionStatusEntity>(new Error(errorMessage, 400))));


        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo(errorMessage));
        Assert.That(result.Error.Code, Is.EqualTo(400));
        await _transactionRepositoryMock.DidNotReceive().CreateAsync(Arg.Any<TransactionEntity>());
        await _transactionProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionDTO>());
    }

    [Test]
    public async Task SendTransactionAsync_CreateTransactionThrowsError_ProducerIsNotCalled()
    {
        var errorMessage = "Error while trying to create the transaction";
        _transactionRepositoryMock.CreateAsync(Arg.Any<TransactionEntity>())
            .Returns(Task.FromResult(new Result<TransactionEntity>(new Error(errorMessage, 400))));

        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo(errorMessage));
        Assert.That(result.Error.Code, Is.EqualTo(400));
        await _transactionRepositoryMock.Received(1).CreateAsync(Arg.Any<TransactionEntity>());
        await _transactionProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionDTO>());
    }

    [Test]
    public async Task SendTransactionAsync_TransactionStatusRepositoryThrowsAnError_ProducerIsNotCalled()
    {
        _transactionStatusRepositoryMock.GetTransactionTypeByName(Arg.Any<TransactionStatusEnum>())
            .ThrowsAsync(new Exception("Exception"));

        var result = await _transactionService.SendTransactionAsync(_transactionToCreate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Message, Is.EqualTo("Unexpected error trying to send the transaction"));
        Assert.That(result.Error.Code, Is.EqualTo(503));
        await _transactionRepositoryMock.DidNotReceive().CreateAsync(Arg.Any<TransactionEntity>());
        await _transactionProducerMock.DidNotReceive().ProduceAsync(Arg.Any<TransactionDTO>());
    }

}
