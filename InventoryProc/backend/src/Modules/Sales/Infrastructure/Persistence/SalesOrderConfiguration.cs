using InventoryProc.Modules.Sales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryProc.Modules.Sales.Infrastructure.Persistence;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");

        builder.HasKey(so => so.Id);

        builder.Property(so => so.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(so => so.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(so => so.SubTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.Notes)
            .HasMaxLength(1000);

        builder.Property(so => so.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(so => so.BillingAddress)
            .HasMaxLength(500);

        builder.Property(so => so.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relationships
        builder.HasOne(so => so.Customer)
            .WithMany()
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(so => so.Items)
            .WithOne(soi => soi.SalesOrder)
            .HasForeignKey(soi => soi.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(so => new { so.TenantId, so.OrderNumber })
            .IsUnique();

        builder.HasIndex(so => so.CustomerId);
        builder.HasIndex(so => so.OrderDate);
        builder.HasIndex(so => so.Status);
    }
}
