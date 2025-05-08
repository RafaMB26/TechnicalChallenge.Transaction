using Transaction.Domain.Entities;

namespace Transaction.Domain.DTOs;

public class TransactionDTO
{
    public Guid TransactionExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public int TransferTypeId { get; set; }
    public int Status { get; set; }
    public decimal Value { get; set; }
}
