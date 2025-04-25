namespace Common.DTOs;

public class TransactionProcessedStatusDTO
{
    public Guid TransactionExternalId { get; set; }
    public bool IsCorrect { get; set; }
    public string RejectedReason { get; set; }
}