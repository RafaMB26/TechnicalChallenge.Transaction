using Common.Enums;
using Common.Result;
using NSubstitute;
using Transaction.Domain.Entities;

namespace Transaction.Application.Test.Services.TransactionServiceTest;

[TestFixture]
public class UpdateTransactionStatusTest : TransactionServiceBase
{
    [Test]
    public async Task UpdateTransactionStatus_TransactionHasBeenApproved_TransactionIsUpdated()
    {
        var approvedTransactionStatus = new TransactionStatusEntity()
        {
            Id = 2,
            Description = "Approved"
        };

        _transactionStatusRepositoryMock.GetTransactionTypeByName(TransactionStatusEnum.Approved)
            .Returns(Task.FromResult(new Result<TransactionStatusEntity>(approvedTransactionStatus)));

        var result = await _transactionService.UpdateTransactionStatus(_transactionToUpdate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.That(result.Data.Status, Is.EqualTo(2));
        await _transactionRepositoryMock.Received(1).UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>());
    }

    [Test]
    public async Task UpdateTransactionStatus_TransactionHasBeenRejected_TransactionIsUpdated()
    {
        _transactionToUpdate.IsCorrect = false;
        var rejectedTransactionStatus = new TransactionStatusEntity()
        {
            Id = 3,
            Description = "Rejected"
        };

        _transactionStatusRepositoryMock.GetTransactionTypeByName(TransactionStatusEnum.Rejected)
            .Returns(Task.FromResult(new Result<TransactionStatusEntity>(rejectedTransactionStatus)));

        var result = await _transactionService.UpdateTransactionStatus(_transactionToUpdate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.That(result.Data.Status, Is.EqualTo(3));
        await _transactionRepositoryMock.Received(1).UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>());
    }

    [Test]
    public async Task UpdateTransactionStatus_TransactionDoesNotExist_TransactionIsNotUpdated()
    {
        var error = "The transaction does not exist.";
        _transactionRepositoryMock.GetTransactionByPublicIdAsync(Arg.Any<Guid>())
            .Returns(new Result<TransactionEntity>(new Error(error, 404)));

        var result = await _transactionService.UpdateTransactionStatus(_transactionToUpdate);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Code, Is.EqualTo(404));
        Assert.That(result.Error.Message, Is.EqualTo(error));
        await _transactionRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>());
    }

    [Test]
    public async Task UpdateTransactionStatus_TransactionStatusRepositoryThrowsError_TransactionIsUpdated()
    {
        var error = "Error while trying to get the transaction type.";
        _transactionStatusRepositoryMock.GetTransactionTypeByName(Arg.Any<TransactionStatusEnum>())
            .Returns(new Result<TransactionStatusEntity>(new Error(error, 404)));

        var result = await _transactionService.UpdateTransactionStatus(_transactionToUpdate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Error);
        Assert.That(result.Data.Status, Is.EqualTo(1));
        await _transactionRepositoryMock.Received(1).UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>());
    }

    [Test]
    public async Task UpdateTransactionStatus_TransactionRepositoryThrowsError_TransactionIsUpdated()
    {
        var error = "Error while trying to update the transaction.";
        _transactionRepositoryMock.UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>())
            .Returns(new Result<TransactionEntity>(new Error(error, 400)));

        var result = await _transactionService.UpdateTransactionStatus(_transactionToUpdate);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Error);
        Assert.That(result.Data.Status, Is.EqualTo(1));
        await _transactionRepositoryMock.Received(1).UpdateAsync(Arg.Any<int>(), Arg.Any<TransactionEntity>());
    }
}
