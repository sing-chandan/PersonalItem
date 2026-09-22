using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Sales.Infrastructure.Persistence;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.BillingAddress)
            .HasMaxLength(500);

        builder.Property(i => i.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(i => i.CustomerGSTNumber)
            .HasMaxLength(15);

        builder.Property(i => i.SalesOrderNumber)
            .HasMaxLength(50);

        builder.Property(i => i.SubTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.ShippingCharges)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.PaidAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.BalanceAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(i => i.PaymentReference)
            .HasMaxLength(100);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.TermsAndConditions)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SalesOrder)
            .WithMany()
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(ip => ip.Invoice)
            .HasForeignKey(ip => ip.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber })
            .IsUnique();

        builder.HasIndex(i => i.CustomerId);
        builder.HasIndex(i => i.SalesOrderId);
        builder.HasIndex(i => i.InvoiceDate);
        builder.HasIndex(i => i.DueDate);
        builder.HasIndex(i => i.PaymentStatus);
    }
}
