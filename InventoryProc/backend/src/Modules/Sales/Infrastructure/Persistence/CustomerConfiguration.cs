using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Sales.Infrastructure.Persistence;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactPerson)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Mobile)
            .HasMaxLength(20);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .HasMaxLength(100);

        builder.Property(c => c.Pincode)
            .HasMaxLength(20);

        builder.Property(c => c.GSTNumber)
            .HasMaxLength(15);

        builder.Property(c => c.PANNumber)
            .HasMaxLength(10);

        builder.Property(c => c.CreditLimit)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.OutstandingBalance)
            .HasColumnType("decimal(18,2)");

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.CustomerCode })
            .IsUnique();

        builder.HasIndex(c => c.CompanyName);
        builder.HasIndex(c => c.Email);
    }
}
