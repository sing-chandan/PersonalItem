using InventoryProc.Modules.Purchases.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Purchases.Infrastructure.Persistence;

public class GoodsReceiptNoteConfiguration : IEntityTypeConfiguration<GoodsReceiptNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNote> builder)
    {
        builder.ToTable("GoodsReceiptNotes");

        builder.HasKey(grn => grn.Id);

        builder.Property(grn => grn.GRNNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(grn => grn.PurchaseOrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(grn => grn.VendorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(grn => grn.InvoiceNumber)
            .HasMaxLength(50);

        builder.Property(grn => grn.ReceivedBy)
            .HasMaxLength(200);

        builder.Property(grn => grn.Notes)
            .HasMaxLength(1000);

        builder.Property(grn => grn.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(grn => grn.PurchaseOrder)
            .WithMany()
            .HasForeignKey(grn => grn.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(grn => grn.Vendor)
            .WithMany()
            .HasForeignKey(grn => grn.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(grn => grn.Items)
            .WithOne(i => i.GoodsReceiptNote)
            .HasForeignKey(i => i.GoodsReceiptNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(grn => new { grn.TenantId, grn.GRNNumber }).IsUnique();
        builder.HasIndex(grn => grn.PurchaseOrderId);
        builder.HasIndex(grn => grn.VendorId);
        builder.HasIndex(grn => grn.ReceiptDate);
        builder.HasIndex(grn => grn.Status);
    }
}
