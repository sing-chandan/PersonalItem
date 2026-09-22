using InventoryProc.Modules.Purchases.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Purchases.Infrastructure.Persistence;

public class GoodsReceiptNoteItemConfiguration : IEntityTypeConfiguration<GoodsReceiptNoteItem>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNoteItem> builder)
    {
        builder.ToTable("GoodsReceiptNoteItems");

        builder.HasKey(grni => grni.Id);

        builder.Property(grni => grni.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(grni => grni.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(grni => grni.OrderedQuantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(grni => grni.ReceivedQuantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(grni => grni.AcceptedQuantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(grni => grni.RejectedQuantity)
            .HasColumnType("decimal(18,3)");

        builder.Property(grni => grni.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(grni => grni.Notes)
            .HasMaxLength(500);

        builder.Property(grni => grni.RejectionReason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(grni => grni.GoodsReceiptNoteId);
        builder.HasIndex(grni => grni.PurchaseOrderItemId);
        builder.HasIndex(grni => grni.ProductId);
    }
}
