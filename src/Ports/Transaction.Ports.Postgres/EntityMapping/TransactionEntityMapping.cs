using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transaction.Domain.Entities;

namespace Transaction.Ports.Postgres.EntityMapping;

public class TransactionEntityMapping : IEntityTypeConfiguration<TransactionEntity>
{
    public void Configure(EntityTypeBuilder<TransactionEntity> builder)
    {
        builder.ToTable("transaction");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TransactionExternalId)
            .HasColumnType("uuid")
            .HasColumnName("transaction_external_id")
            .IsRequired();

        builder.HasIndex(d => d.TransactionExternalId)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.SourceAccountId)
            .HasColumnType("uuid")
            .HasColumnName("source_account_id")
            .IsRequired();

        builder.Property(x => x.TargetAccountId)
            .HasColumnType("uuid")
            .HasColumnName("target_account_id")
            .IsRequired();

        builder.Property(x => x.TransferTypeId)
            .HasColumnType("integer")
            .HasColumnName("transfer_type_id")
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnType("numeric")
            .HasColumnName("value")
            .IsRequired();

        builder.HasOne(d => d.Status)
            .WithMany(d => d.Transactions)
            .HasForeignKey(d => d.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
