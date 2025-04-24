namespace Transaction.Domain.Entities;

public class TransactionEntity :Entity
{
    public Guid TransactionExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public int TransferTypeId { get; set; }
    public int StatusId { get; set; }
    public TransactionStatusEntity Status { get; set; }
    public decimal Value { get; set; }
}
