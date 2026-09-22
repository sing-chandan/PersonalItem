using InventoryProc.Modules.Purchases.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Purchases.Infrastructure.Persistence;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");

        builder.HasKey(poi => poi.Id);

        builder.Property(poi => poi.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(poi => poi.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(poi => poi.Quantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(poi => poi.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(poi => poi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(poi => poi.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(poi => poi.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(poi => poi.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(poi => poi.ReceivedQuantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(poi => poi.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(poi => poi.PurchaseOrderId);
        builder.HasIndex(poi => poi.ProductId);
    }
}
