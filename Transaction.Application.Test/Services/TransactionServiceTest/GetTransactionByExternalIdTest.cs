using Common.Result;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Transaction.Domain.Entities;

namespace Transaction.Application.Test.Services.TransactionServiceTest;

[TestFixture]
public class GetTransactionByExternalIdTest : TransactionServiceBase
{
    [Test]
    public async Task GetTransactionByExternalId_ExternalIdExists_ReturnsTransaction()
    {
        var result = await _transactionService.GetTransactionByExternalIdAsync(Guid.NewGuid());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.That(result.Data.Value, Is.EqualTo(200));
    }

    [Test]
    public async Task GetTransactionByExternalId_ExternalIdDoesNotExist_ReturnsNotFoundError()
    {
        _transactionRepositoryMock.GetTransactionByPublicIdAsync(Arg.Any<Guid>())
           .Returns(new Result<TransactionEntity>(default(TransactionEntity)));

        var result = await _transactionService.GetTransactionByExternalIdAsync(Guid.NewGuid());

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Code, Is.EqualTo(404));
        Assert.IsTrue(result.Error.Message.EndsWith("does not exist."));
    }

    [Test]
    public async Task GetTransactionByExternalId_RepositoryThrowsError_ReturnGeneralError()
    {
        _transactionRepositoryMock.GetTransactionByPublicIdAsync(Arg.Any<Guid>())
           .ThrowsAsync(new Exception("Test Exception"));

        var result = await _transactionService.GetTransactionByExternalIdAsync(Guid.NewGuid());

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Error);
        Assert.That(result.Error.Code, Is.EqualTo(503));
        Assert.IsTrue(result.Error.Message.StartsWith("Unexpected error trying to get the transaction"));
    }
}
