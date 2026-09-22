using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Sales.Infrastructure.Persistence;

public class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
{
    public void Configure(EntityTypeBuilder<InvoicePayment> builder)
    {
        builder.ToTable("InvoicePayments");

        builder.HasKey(ip => ip.Id);

        builder.Property(ip => ip.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(ip => ip.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ip => ip.Reference)
            .HasMaxLength(100);

        builder.Property(ip => ip.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(ip => ip.InvoiceId);
        builder.HasIndex(ip => ip.PaymentDate);
    }
}
