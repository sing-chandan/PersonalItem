using Microsoft.EntityFrameworkCore;
using InventoryProc.SharedKernel.Common;
using InventoryProc.SharedKernel.Services;
using InventoryProc.Modules.Tenancy.Domain.Entities;
using InventoryProc.Modules.Identity.Domain.Entities;
using InventoryProc.Modules.Products.Domain.Entities;
using InventoryProc.Modules.Sales.Domain.Entities;
using InventoryProc.Modules.Purchases.Domain.Entities;
using System.Linq.Expressions;
using System.Reflection;

namespace InventoryProc.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentTenantService? _currentTenantService;
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    // Method for tenant filter - evaluated at query execution time
    private Guid GetCurrentTenantId() => _currentTenantService?.TenantId ?? Guid.Empty;

    // DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems => Set<GoodsReceiptNoteItem>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // Additional DbSets will be added as modules are implemented

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from all assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply configurations from Sales module
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryProc.Modules.Sales.Domain.Entities.Customer).Assembly);

        // Apply configurations from Purchases module
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryProc.Modules.Purchases.Domain.Entities.Vendor).Assembly);

        // Apply global query filter for tenant isolation
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Apply tenant filter to entities that implement ITenantEntity
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

                // Create expression that calls GetCurrentTenantId method
                // This will be evaluated at query execution time, not at DbContext construction
                var dbContextInstance = Expression.Constant(this);
                var getTenantIdMethod = typeof(ApplicationDbContext).GetMethod(
                    nameof(GetCurrentTenantId),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var tenantIdMethodCall = Expression.Call(dbContextInstance, getTenantIdMethod!);

                var filter = Expression.Lambda(Expression.Equal(property, tenantIdMethodCall), parameter);

                entityType.SetQueryFilter(filter);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set TenantId for new tenant-owned entities (only if not already set)
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && _currentTenantService != null)
            {
                // Only set TenantId if it hasn't been explicitly set (useful for seeding)
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _currentTenantService.TenantId;
                }
            }
        }

        // Auto-set audit fields
        var now = DateTime.UtcNow;
        var userId = _currentUserService?.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
