using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryProc.Modules.Identity.Domain.Entities;

namespace InventoryProc.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.UserEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .HasMaxLength(50);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        // Indexes for efficient querying
        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_ActivityLogs_TenantId");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_ActivityLogs_UserId");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_ActivityLogs_Timestamp");

        builder.HasIndex(a => new { a.TenantId, a.UserId, a.Timestamp })
            .HasDatabaseName("IX_ActivityLogs_TenantId_UserId_Timestamp");

        builder.HasIndex(a => new { a.TenantId, a.EntityType, a.EntityId })
            .HasDatabaseName("IX_ActivityLogs_TenantId_EntityType_EntityId");
    }
}
