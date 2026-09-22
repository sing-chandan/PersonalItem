using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Sales.Infrastructure.Persistence;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ii => ii.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ii => ii.Description)
            .HasMaxLength(500);

        builder.Property(ii => ii.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ii => ii.Quantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(ii => ii.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(ii => ii.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(ii => ii.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(ii => ii.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(ii => ii.HSNCode)
            .HasMaxLength(20);

        // Indexes
        builder.HasIndex(ii => ii.InvoiceId);
        builder.HasIndex(ii => ii.ProductId);
    }
}
