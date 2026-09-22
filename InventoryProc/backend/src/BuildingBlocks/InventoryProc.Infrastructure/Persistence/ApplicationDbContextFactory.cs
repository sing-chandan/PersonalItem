using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InventoryProc.Infrastructure.Persistence;

/// <summary>
/// Factory for creating DbContext at design-time (for migrations)
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use SQL Server for migrations by default (can be changed for PostgreSQL)
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=InventoryProc;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("InventoryProc.Infrastructure"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
