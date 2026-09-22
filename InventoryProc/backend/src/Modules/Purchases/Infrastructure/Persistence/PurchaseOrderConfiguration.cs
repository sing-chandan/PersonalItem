using InventoryProc.Modules.Purchases.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Purchases.Infrastructure.Persistence;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(po => po.VendorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(po => po.SubTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(po => po.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(po => po.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(po => po.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(po => po.Notes)
            .HasMaxLength(1000);

        builder.Property(po => po.DeliveryAddress)
            .HasMaxLength(500);

        builder.Property(po => po.BillingAddress)
            .HasMaxLength(500);

        builder.Property(po => po.ApprovedBy)
            .HasMaxLength(200);

        builder.Property(po => po.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(po => po.Vendor)
            .WithMany()
            .HasForeignKey(po => po.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(po => new { po.TenantId, po.OrderNumber }).IsUnique();
        builder.HasIndex(po => po.VendorId);
        builder.HasIndex(po => po.Status);
        builder.HasIndex(po => po.OrderDate);
    }
}
