using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Sales.Infrastructure.Persistence;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");

        builder.HasKey(soi => soi.Id);

        builder.Property(soi => soi.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(soi => soi.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(soi => soi.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(soi => soi.Quantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(soi => soi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(soi => soi.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(soi => soi.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(soi => soi.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(soi => soi.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(soi => soi.SalesOrderId);
        builder.HasIndex(soi => soi.ProductId);
    }
}
