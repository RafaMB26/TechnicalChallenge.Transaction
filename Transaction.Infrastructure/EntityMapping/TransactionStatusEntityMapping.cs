using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transaction.Domain.Entities;

namespace Transaction.Infrastructure.EntityMapping;

public class TransactionStatusEntityMapping : IEntityTypeConfiguration<TransactionStatusEntity>
{
    public void Configure(EntityTypeBuilder<TransactionStatusEntity> builder)
    {
        builder.ToTable("cat_transaction_status");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Description)
            .HasColumnType("text")
            .HasColumnName("description")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(d => d.Description)
            .IsUnique();
    }
}
