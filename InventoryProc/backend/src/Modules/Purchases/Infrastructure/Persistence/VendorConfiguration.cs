using InventoryProc.Modules.Purchases.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Purchases.Infrastructure.Persistence;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.ContactPerson)
            .HasMaxLength(100);

        builder.Property(v => v.Phone)
            .HasMaxLength(20);

        builder.Property(v => v.Mobile)
            .HasMaxLength(20);

        builder.Property(v => v.Address)
            .HasMaxLength(500);

        builder.Property(v => v.City)
            .HasMaxLength(100);

        builder.Property(v => v.State)
            .HasMaxLength(100);

        builder.Property(v => v.Country)
            .HasMaxLength(100);

        builder.Property(v => v.Pincode)
            .HasMaxLength(20);

        builder.Property(v => v.GSTNumber)
            .HasMaxLength(50);

        builder.Property(v => v.PANNumber)
            .HasMaxLength(50);

        builder.Property(v => v.TaxIdentificationNumber)
            .HasMaxLength(50);

        builder.Property(v => v.CreditLimit)
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.OutstandingBalance)
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.BankName)
            .HasMaxLength(200);

        builder.Property(v => v.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(v => v.BankIFSCCode)
            .HasMaxLength(20);

        builder.Property(v => v.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(v => new { v.TenantId, v.Code }).IsUnique();
        builder.HasIndex(v => v.CompanyName);
        builder.HasIndex(v => v.Email);
    }
}
