﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Transaction.Infrastructure;

#nullable disable

namespace Transaction.Infrastructure.Migrations
{
    [DbContext(typeof(TransactionDbContext))]
    [Migration("20250424175636_populateCatalogues")]
    partial class populateCatalogues
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Transaction.Domain.Entities.TransactionEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid>("SourceAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("source_account_id");

                    b.Property<int>("StatusId")
                        .HasColumnType("integer");

                    b.Property<Guid>("TargetAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("target_account_id");

                    b.Property<Guid>("TransactionExternalId")
                        .HasColumnType("uuid")
                        .HasColumnName("transaction_external_id");

                    b.Property<int>("TransferTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("transfer_type_id");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.HasIndex("StatusId");

                    b.HasIndex("TransactionExternalId")
                        .IsUnique();

                    b.ToTable("transaction", (string)null);
                });

            modelBuilder.Entity("Transaction.Domain.Entities.TransactionStatusEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.HasKey("Id");

                    b.HasIndex("Description")
                        .IsUnique();

                    b.ToTable("cat_transaction_status", (string)null);
                });

            modelBuilder.Entity("Transaction.Domain.Entities.TransactionEntity", b =>
                {
                    b.HasOne("Transaction.Domain.Entities.TransactionStatusEntity", "Status")
                        .WithMany("Transactions")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Transaction.Domain.Entities.TransactionStatusEntity", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
