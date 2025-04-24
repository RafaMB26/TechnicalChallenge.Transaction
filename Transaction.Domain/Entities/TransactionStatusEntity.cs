namespace Transaction.Domain.Entities;

public class TransactionStatusEntity: Entity
{
    public string Description { get; set; }
    public List<TransactionEntity> Transactions { get; set; }
}
