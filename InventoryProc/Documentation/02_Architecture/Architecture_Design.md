# Architecture & Technical Design Document
## Wholesale/Distributor Business Management SaaS

**Document Version:** 1.0
**Date:** 2026-08-29
**Status:** Living Document
**Project:** InventoryProc - Wholesale Distributor SaaS MVP

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-08-29 | Development Team | Initial architecture document |

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Modular Monolith Design](#2-modular-monolith-design)
3. [Technology Stack](#3-technology-stack)
4. [Backend Architecture](#4-backend-architecture)
5. [Frontend Architecture](#5-frontend-architecture)
6. [Database Architecture](#6-database-architecture)
7. [Module Design](#7-module-design)
8. [Cross-Cutting Concerns](#8-cross-cutting-concerns)
9. [Security Architecture](#9-security-architecture)
10. [Deployment Architecture](#10-deployment-architecture)
11. [Future Evolution Path](#11-future-evolution-path)

---

## 1. Architecture Overview

### 1.1 Architecture Style
**Modular Monolith** - Single deployable application with strong logical module boundaries

### 1.2 Key Architectural Principles

1. **Module Isolation**: Each module has clear boundaries and owns its domain
2. **Contract-Based Communication**: Modules communicate through well-defined interfaces
3. **Tenant Isolation**: Complete data and security isolation per tenant
4. **Database Portability**: Support both SQL Server (dev) and PostgreSQL (prod)
5. **Microservice Ready**: Module boundaries designed for future extraction if needed
6. **Security First**: Authentication, authorization, and tenant isolation at every layer
7. **Testability**: Clean architecture enables comprehensive testing

### 1.3 High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                              │
│  ┌──────────────────────┐      ┌──────────────────────┐         │
│  │   Angular Web App    │      │   Flutter Mobile App  │         │
│  │   (TypeScript)       │      │   (Dart)              │         │
│  └──────────────────────┘      └──────────────────────┘         │
└─────────────────────────────────────────────────────────────────┘
                              │
                         HTTPS/REST
                              │
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET CORE API LAYER                        │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Authentication & Authorization Middleware                │   │
│  │  Tenant Resolution Middleware                             │   │
│  │  Exception Handling & Logging Middleware                  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    API CONTROLLERS                        │   │
│  │  (Tenancy, Identity, Products, Customers, Suppliers,     │   │
│  │   Inventory, Purchases, Sales, Payments, Reports)         │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                      APPLICATION LAYER                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │         Module Application Services                      │    │
│  │  • Commands (Create, Update, Delete)                     │    │
│  │  • Queries (Get, List, Search)                           │    │
│  │  • Business Workflows                                    │    │
│  │  • DTOs & Mapping                                        │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                       DOMAIN LAYER                               │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Module Domain Models                        │    │
│  │  • Entities (Product, Customer, Sale, etc.)              │    │
│  │  • Value Objects                                         │    │
│  │  • Domain Services                                       │    │
│  │  • Business Rules                                        │    │
│  │  • Domain Events (future)                                │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Data Access & Persistence                   │    │
│  │  • EF Core DbContext                                     │    │
│  │  • Repositories (selective)                              │    │
│  │  • Query Services                                        │    │
│  │  • Migrations (SQL Server & PostgreSQL)                  │    │
│  └─────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              External Services                           │    │
│  │  • File Storage                                          │    │
│  │  • Email Service (future)                                │    │
│  │  • SMS Service (future)                                  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                        DATABASE LAYER                            │
│  ┌──────────────────────┐      ┌──────────────────────┐         │
│  │   SQL Server         │      │   PostgreSQL         │         │
│  │   (Development)      │      │   (Production)       │         │
│  └──────────────────────┘      └──────────────────────┘         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Modular Monolith Design

### 2.1 Why Modular Monolith?

**Advantages for MVP:**
- ✅ Simple deployment (single artifact)
- ✅ Easier development and debugging
- ✅ No distributed system complexity
- ✅ Lower infrastructure costs
- ✅ Faster initial development
- ✅ One database transaction boundary
- ✅ Strong module boundaries prepare for future extraction

**Future Path:**
- Modules can be extracted to microservices when justified by scale/team/operations

### 2.2 Module Structure

```
┌─────────────────────────────────────────────────────────┐
│              ASP.NET CORE APPLICATION                    │
│                                                          │
│  ┌────────────────────────────────────────────────┐     │
│  │              BUILDING BLOCKS                    │     │
│  │  • SharedKernel (Common Types, Base Classes)   │     │
│  │  • Contracts (Inter-module Interfaces)         │     │
│  │  • Infrastructure (EF Core, Logging, etc.)     │     │
│  └────────────────────────────────────────────────┘     │
│                                                          │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐           │
│  │ Tenancy   │  │ Identity  │  │ Products  │           │
│  │ Module    │  │ Module    │  │ Module    │           │
│  └───────────┘  └───────────┘  └───────────┘           │
│                                                          │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐           │
│  │ Customers │  │ Suppliers │  │ Inventory │           │
│  │ Module    │  │ Module    │  │ Module    │           │
│  └───────────┘  └───────────┘  └───────────┘           │
│                                                          │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐           │
│  │ Purchases │  │  Sales    │  │ Payments  │           │
│  │ Module    │  │  Module   │  │ Module    │           │
│  └───────────┘  └───────────┘  └───────────┘           │
│                                                          │
│  ┌───────────┐  ┌───────────┐                           │
│  │ Reporting │  │  Imports  │                           │
│  │ Module    │  │  Module   │                           │
│  └───────────┘  └───────────┘                           │
│                                                          │
│           ONE DEPLOYMENT • ONE DATABASE                  │
└─────────────────────────────────────────────────────────┘
```

### 2.3 Module Communication Rules

#### ✅ Allowed
- Module A → Interface (Contract) → Module B implementation
- Module A → Shared Kernel types
- Module A → Building Blocks infrastructure

#### ❌ Prohibited
- Module A → Module B repository directly
- Module A → Module B database tables directly
- Module A → Module B internal implementation
- Circular module dependencies

#### Example: Sales Module Creating a Sale

```
Sales Module
    ├─> ICustomerService.GetCustomer(customerId)      [Customers Module Contract]
    ├─> IProductService.GetProduct(productId)         [Products Module Contract]
    ├─> IInventoryService.ValidateStock(productId)    [Inventory Module Contract]
    ├─> ISalesRepository.CreateSale(sale)             [Sales Module Repository]
    ├─> IInventoryService.DecreaseStock(...)          [Inventory Module Contract]
    └─> ICustomerLedgerService.CreateEntry(...)       [Payments Module Contract]
```

---

## 3. Technology Stack

### 3.1 LOCKED Technology Stack

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Web Frontend** | Angular | 18+ | Modern, TypeScript, enterprise-ready |
| **Mobile** | Flutter | 3.x+ | Cross-platform, native performance |
| **Backend** | ASP.NET Core | .NET 8 | Mature, performant, excellent tooling |
| **Language** | C# | 12 | Strong typing, modern features |
| **ORM** | Entity Framework Core | 8.x | Code-first, migrations, LINQ |
| **Dev Database** | SQL Server | 2019+ | Local development, Windows compatibility |
| **Prod Database** | PostgreSQL | 15+ | Cost-effective, reliable, cloud-friendly |
| **API** | REST/JSON | - | Standard, simple, widely supported |
| **Authentication** | JWT + ASP.NET Identity | - | Industry standard |
| **API Docs** | Swagger/OpenAPI | 3.x | Auto-generated, interactive |

### 3.2 Supporting Libraries & Tools

**Backend:**
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Logging:** Serilog
- **Testing:** xUnit, Moq, FluentAssertions
- **Excel:** EPPlus or ClosedXML
- **PDF:** QuestPDF or DinkToPdf

**Frontend (Angular):**
- **UI Library:** Angular Material or PrimeNG
- **State Management:** NgRx or Akita (if needed)
- **HTTP:** HttpClient (built-in)
- **Forms:** Reactive Forms (built-in)
- **Charts:** ng2-charts or Chart.js

**Mobile (Flutter):**
- **State Management:** Provider or Riverpod
- **HTTP:** dio
- **Local Storage:** shared_preferences
- **Charts:** fl_chart

**DevOps:**
- **Containerization:** Docker
- **CI/CD:** GitHub Actions or Azure DevOps
- **Reverse Proxy:** Nginx
- **Monitoring:** Application Insights or similar

---

## 4. Backend Architecture

### 4.1 Project Structure

```
backend/
├── src/
│   ├── Host/
│   │   └── InventoryProc.API/                    # Main API project (startup)
│   │       ├── Controllers/                      # API endpoints
│   │       ├── Middleware/                       # Custom middleware
│   │       ├── Filters/                          # Action filters
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       └── Program.cs
│   │
│   ├── BuildingBlocks/
│   │   ├── SharedKernel/                         # Common types, interfaces
│   │   │   ├── Common/
│   │   │   │   ├── Entity.cs
│   │   │   │   ├── IAuditableEntity.cs
│   │   │   │   ├── ITenantEntity.cs
│   │   │   │   └── Result.cs
│   │   │   ├── Exceptions/
│   │   │   └── Extensions/
│   │   │
│   │   ├── Contracts/                            # Inter-module contracts
│   │   │   ├── ICustomerService.cs
│   │   │   ├── IInventoryService.cs
│   │   │   └── ...
│   │   │
│   │   └── Infrastructure/                       # Shared infrastructure
│   │       ├── Persistence/
│   │       │   ├── ApplicationDbContext.cs       # Main EF Core DbContext
│   │       │   ├── ApplicationDbContextFactory.cs
│   │       │   └── Migrations/
│   │       ├── Identity/
│   │       ├── Logging/
│   │       └── Services/
│   │
│   └── Modules/
│       ├── Tenancy/
│       │   ├── Domain/
│       │   │   ├── Entities/
│       │   │   │   └── Tenant.cs
│       │   │   └── Services/
│       │   ├── Application/
│       │   │   ├── Commands/
│       │   │   ├── Queries/
│       │   │   ├── DTOs/
│       │   │   └── Mappings/
│       │   ├── Infrastructure/
│       │   │   ├── Repositories/
│       │   │   └── Services/
│       │   └── Api/
│       │       └── Controllers/
│       │           └── TenantController.cs
│       │
│       ├── Identity/
│       │   ├── Domain/
│       │   ├── Application/
│       │   ├── Infrastructure/
│       │   └── Api/
│       │
│       ├── Products/
│       │   ├── Domain/
│       │   │   ├── Entities/
│       │   │   │   ├── Product.cs
│       │   │   │   ├── Category.cs
│       │   │   │   ├── Brand.cs
│       │   │   │   └── Unit.cs
│       │   │   └── Services/
│       │   ├── Application/
│       │   │   ├── Commands/
│       │   │   │   ├── CreateProductCommand.cs
│       │   │   │   ├── UpdateProductCommand.cs
│       │   │   │   └── DeleteProductCommand.cs
│       │   │   ├── Queries/
│       │   │   │   ├── GetProductQuery.cs
│       │   │   │   └── ListProductsQuery.cs
│       │   │   ├── DTOs/
│       │   │   │   ├── ProductDto.cs
│       │   │   │   └── CreateProductRequest.cs
│       │   │   └── Services/
│       │   │       └── ProductService.cs
│       │   ├── Infrastructure/
│       │   │   ├── Repositories/
│       │   │   │   └── ProductRepository.cs
│       │   │   ├── EntityConfigurations/
│       │   │   │   └── ProductConfiguration.cs
│       │   │   └── Services/
│       │   └── Api/
│       │       └── Controllers/
│       │           └── ProductsController.cs
│       │
│       ├── Customers/
│       ├── Suppliers/
│       ├── Inventory/
│       ├── Purchases/
│       ├── Sales/
│       ├── Payments/
│       ├── Reporting/
│       └── Imports/
│
└── tests/
    ├── UnitTests/
    │   ├── Products.UnitTests/
    │   ├── Sales.UnitTests/
    │   └── ...
    ├── IntegrationTests/
    │   ├── Products.IntegrationTests/
    │   ├── Api.IntegrationTests/
    │   └── ...
    └── TestHelpers/
```

### 4.2 Module Internal Structure (Clean Architecture)

Each module follows clean architecture layers:

```
ModuleName/
├── Domain/                          # Pure business logic, no dependencies
│   ├── Entities/                    # Domain entities
│   ├── ValueObjects/                # Value objects (future)
│   ├── Enums/                       # Enumerations
│   ├── Exceptions/                  # Domain exceptions
│   └── Services/                    # Domain services
│
├── Application/                     # Use cases, application logic
│   ├── Commands/                    # Write operations (CQRS-style)
│   │   ├── CreateXCommand.cs
│   │   └── CreateXCommandHandler.cs
│   ├── Queries/                     # Read operations (CQRS-style)
│   │   ├── GetXQuery.cs
│   │   └── GetXQueryHandler.cs
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Validators/                  # FluentValidation validators
│   ├── Mappings/                    # AutoMapper profiles
│   └── Services/                    # Application services
│
├── Infrastructure/                  # External concerns (DB, files, etc.)
│   ├── Persistence/
│   │   ├── Repositories/            # Data access implementations
│   │   └── EntityConfigurations/    # EF Core configurations
│   └── Services/                    # Infrastructure services
│
└── Api/                             # API endpoints
    └── Controllers/                 # REST controllers
```

### 4.3 Request/Response Flow

```
HTTP Request
    │
    ↓
[Middleware Pipeline]
    ├─> Authentication
    ├─> Tenant Resolution
    ├─> Exception Handling
    ├─> Logging
    │
    ↓
[API Controller]
    ├─> Request validation
    ├─> Map Request → Command/Query
    │
    ↓
[Application Layer]
    ├─> Command/Query Handler
    ├─> Business validation (FluentValidation)
    ├─> Business logic execution
    ├─> Call domain services
    ├─> Call repositories/query services
    │
    ↓
[Infrastructure Layer]
    ├─> Repository/Query Service
    ├─> EF Core DbContext
    │
    ↓
[Database]
    ├─> Execute SQL
    │
    ↓
[Return Response]
    ├─> Map Domain → DTO
    ├─> Return Result<T>
    │
    ↓
HTTP Response (JSON)
```

### 4.4 Dependency Injection Configuration

```csharp
// Program.cs - Modular service registration

var builder = WebApplication.CreateBuilder(args);

// Building Blocks
builder.Services.AddSharedKernel();
builder.Services.AddInfrastructure(builder.Configuration);

// Modules
builder.Services.AddTenancyModule();
builder.Services.AddIdentityModule();
builder.Services.AddProductsModule();
builder.Services.AddCustomersModule();
builder.Services.AddSuppliersModule();
builder.Services.AddInventoryModule();
builder.Services.AddPurchasesModule();
builder.Services.AddSalesModule();
builder.Services.AddPaymentsModule();
builder.Services.AddReportingModule();
builder.Services.AddImportsModule();

var app = builder.Build();

// Middleware pipeline
app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();
app.UseExceptionHandling();

app.MapControllers();
app.Run();
```

---

## 5. Frontend Architecture

### 5.1 Angular Project Structure

```
frontend-angular/
├── src/
│   ├── app/
│   │   ├── core/                           # Singleton services, guards
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── token.interceptor.ts
│   │   │   ├── http/
│   │   │   │   ├── api.service.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── tenant/
│   │   │   │   └── tenant.service.ts
│   │   │   └── layout/
│   │   │       ├── header/
│   │   │       ├── sidebar/
│   │   │       └── footer/
│   │   │
│   │   ├── shared/                         # Reusable components
│   │   │   ├── components/
│   │   │   │   ├── data-table/
│   │   │   │   ├── search-box/
│   │   │   │   ├── confirm-dialog/
│   │   │   │   └── loader/
│   │   │   ├── directives/
│   │   │   ├── pipes/
│   │   │   └── models/
│   │   │       ├── product.model.ts
│   │   │       ├── customer.model.ts
│   │   │       └── ...
│   │   │
│   │   ├── features/                       # Feature modules
│   │   │   ├── dashboard/
│   │   │   │   ├── dashboard.component.ts
│   │   │   │   └── dashboard.service.ts
│   │   │   │
│   │   │   ├── products/
│   │   │   │   ├── product-list/
│   │   │   │   ├── product-form/
│   │   │   │   ├── product-detail/
│   │   │   │   └── products.service.ts
│   │   │   │
│   │   │   ├── customers/
│   │   │   ├── suppliers/
│   │   │   ├── inventory/
│   │   │   ├── purchases/
│   │   │   │   ├── purchase-list/
│   │   │   │   ├── purchase-form/
│   │   │   │   └── purchases.service.ts
│   │   │   │
│   │   │   ├── sales/
│   │   │   │   ├── sales-list/
│   │   │   │   ├── sales-form/
│   │   │   │   ├── invoice/
│   │   │   │   └── sales.service.ts
│   │   │   │
│   │   │   ├── payments/
│   │   │   ├── reports/
│   │   │   │   ├── sales-reports/
│   │   │   │   ├── purchase-reports/
│   │   │   │   ├── inventory-reports/
│   │   │   │   └── reports.service.ts
│   │   │   │
│   │   │   └── imports/
│   │   │       ├── import-products/
│   │   │       ├── import-customers/
│   │   │       └── imports.service.ts
│   │   │
│   │   ├── app.routes.ts                   # Application routes
│   │   ├── app.component.ts
│   │   └── app.config.ts
│   │
│   ├── assets/
│   │   ├── images/
│   │   ├── icons/
│   │   └── styles/
│   │
│   └── environments/
│       ├── environment.ts
│       └── environment.prod.ts
│
├── angular.json
├── package.json
└── tsconfig.json
```

### 5.2 Flutter Project Structure

```
mobile-flutter/
├── lib/
│   ├── core/
│   │   ├── networking/
│   │   │   ├── api_client.dart
│   │   │   └── api_endpoints.dart
│   │   ├── auth/
│   │   │   ├── auth_service.dart
│   │   │   └── token_storage.dart
│   │   ├── storage/
│   │   │   └── local_storage.dart
│   │   ├── routing/
│   │   │   └── app_router.dart
│   │   └── theme/
│   │       └── app_theme.dart
│   │
│   ├── shared/
│   │   ├── widgets/
│   │   │   ├── custom_app_bar.dart
│   │   │   ├── loading_indicator.dart
│   │   │   └── error_widget.dart
│   │   └── models/
│   │       ├── product.dart
│   │       ├── customer.dart
│   │       └── ...
│   │
│   ├── features/
│   │   ├── dashboard/
│   │   │   ├── dashboard_screen.dart
│   │   │   └── dashboard_provider.dart
│   │   │
│   │   ├── customers/
│   │   │   ├── customer_list_screen.dart
│   │   │   ├── customer_detail_screen.dart
│   │   │   └── customers_provider.dart
│   │   │
│   │   ├── products/
│   │   ├── sales/
│   │   ├── payments/
│   │   └── reports/
│   │
│   └── main.dart
│
├── pubspec.yaml
└── analysis_options.yaml
```

---

## 6. Database Architecture

### 6.1 Database Portability Strategy

**Requirement:** Support both SQL Server (development) and PostgreSQL (production)

**Strategy:**
1. Use EF Core portable features only
2. Avoid database-specific SQL in business logic
3. Isolate provider-specific code in Infrastructure layer
4. Maintain dual migrations (SQL Server + PostgreSQL)
5. Run integration tests on both databases

### 6.2 Database Design Principles

1. **Tenant Isolation:** Every tenant-owned table includes `TenantId`
2. **Audit Trail:** All transactional entities include `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
3. **Soft Delete:** Master data uses `IsActive` flag instead of hard delete
4. **Immutability:** Transaction records (Sale, Purchase, Payment) are immutable
5. **Referential Integrity:** Foreign keys enforced at database level
6. **Indexes:** Strategic indexes on frequently queried fields + `TenantId`

### 6.3 EF Core DbContext Design

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    // DbSets for all modules
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<StockBalance> StockBalances { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CustomerLedgerEntry> CustomerLedgerEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply entity configurations from all modules
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filter for tenant isolation
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = SetGlobalQueryMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    // Auto-set TenantId and audit fields on save
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.TenantId;
        var userId = _currentUserService.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
            }
        }

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
```

### 6.4 Migration Strategy

**Development:**
```bash
# SQL Server migrations
dotnet ef migrations add InitialCreate --context ApplicationDbContext --provider SqlServer
dotnet ef database update
```

**Production:**
```bash
# PostgreSQL migrations
dotnet ef migrations add InitialCreate --context ApplicationDbContext --provider Npgsql
dotnet ef database update
```

**Note:** Use separate migration folders or configurations for each provider if needed.

---

## 7. Module Design

### 7.1 Module Dependency Graph

```
                    ┌─────────────┐
                    │   Tenancy   │
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │   Identity  │
                    └──────┬──────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼────┐      ┌─────▼──────┐     ┌────▼────┐
   │Products │      │ Customers  │     │Suppliers│
   └────┬────┘      └─────┬──────┘     └────┬────┘
        │                  │                  │
        └──────────────────┼──────────────────┘
                           │
                    ┌──────▼──────┐
                    │  Inventory  │
                    └──────┬──────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼────┐      ┌─────▼──────┐     ┌────▼────┐
   │Purchases│      │   Sales    │     │Payments │
   └─────────┘      └────────────┘     └────┬────┘
                                             │
                    ┌────────────────────────┘
                    │
             ┌──────▼──────┐
             │  Reporting  │
             └─────────────┘
```

**Key Rules:**
- No circular dependencies
- Higher-level modules depend on lower-level modules
- All modules can depend on BuildingBlocks (SharedKernel, Contracts, Infrastructure)
- Modules communicate via contracts, not direct implementation

### 7.2 Module Contract Examples

#### ICustomerService (Contracts)
```csharp
public interface ICustomerService
{
    Task<CustomerDto> GetCustomerAsync(Guid customerId);
    Task<decimal> GetOutstandingBalanceAsync(Guid customerId);
    Task<bool> IsCustomerActiveAsync(Guid customerId);
}
```

#### IInventoryService (Contracts)
```csharp
public interface IInventoryService
{
    Task<int> GetAvailableStockAsync(Guid productId);
    Task<bool> ValidateStockAvailabilityAsync(Guid productId, int requiredQuantity);
    Task IncreaseStockAsync(Guid productId, int quantity, string referenceType, Guid referenceId);
    Task DecreaseStockAsync(Guid productId, int quantity, string referenceType, Guid referenceId);
    Task<List<LowStockProductDto>> GetLowStockProductsAsync();
}
```

#### IProductService (Contracts)
```csharp
public interface IProductService
{
    Task<ProductDto> GetProductAsync(Guid productId);
    Task<bool> IsProductActiveAsync(Guid productId);
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
}
```

---

## 8. Cross-Cutting Concerns

### 8.1 Authentication & Authorization

**JWT Token Structure:**
```json
{
  "sub": "user-id-guid",
  "email": "user@example.com",
  "tenantId": "tenant-id-guid",
  "role": "Admin",
  "exp": 1672531200,
  "iss": "InventoryProc.API",
  "aud": "InventoryProc.Client"
}
```

**Authorization Flow:**
```
Request with JWT Token
    │
    ↓
[Authentication Middleware]
    ├─> Validate JWT signature
    ├─> Extract user identity & tenant
    │
    ↓
[Tenant Resolution Middleware]
    ├─> Set CurrentTenantService.TenantId
    │
    ↓
[Authorization Middleware]
    ├─> Check user role & permissions
    │
    ↓
[Controller Action]
    ├─> Access CurrentTenantService for tenant context
    ├─> Access CurrentUserService for user context
```

### 8.2 Tenant Resolution

**ITenantService Implementation:**
```csharp
public class TenantService : ICurrentTenantService
{
    private Guid? _tenantId;

    public Guid TenantId
    {
        get => _tenantId ?? throw new InvalidOperationException("Tenant not set");
        set => _tenantId = value;
    }

    public bool IsSet => _tenantId.HasValue;
}

// Middleware
public class TenantResolutionMiddleware
{
    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        var tenantId = context.User.FindFirst("tenantId")?.Value;

        if (Guid.TryParse(tenantId, out var parsedTenantId))
        {
            tenantService.TenantId = parsedTenantId;
        }

        await _next(context);
    }
}
```

### 8.3 Exception Handling

**Global Exception Handler:**
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "ValidationError",
                errors = ex.Errors
            });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "NotFound",
                message = ex.Message
            });
        }
        catch (BusinessRuleException ex)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "BusinessRuleViolation",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "InternalServerError",
                message = "An unexpected error occurred"
            });
        }
    }
}
```

### 8.4 Logging

**Structured Logging with Serilog:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Application", "InventoryProc.API")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")  // Optional
    .CreateLogger();

// In code
_logger.LogInformation("Creating sale for Customer {CustomerId}, Amount {Amount}",
    customerId, amount);
```

### 8.5 Validation

**FluentValidation Example:**
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200);

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50);

        RuleFor(x => x.SellingPrice)
            .GreaterThan(0).WithMessage("Selling price must be greater than zero");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0);
    }
}
```

---

## 9. Security Architecture

### 9.1 Security Layers

```
┌─────────────────────────────────────────┐
│         Transport Security (HTTPS)       │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│      Authentication (JWT Token)          │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│    Tenant Isolation (Query Filters)      │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│   Authorization (Role-Based Access)      │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│      Input Validation (DTOs, Fluent)     │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│    Business Logic (Domain Rules)         │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│   Data Access (EF Core, Parameterized)   │
└─────────────────────────────────────────┘
```

### 9.2 Tenant Isolation Checklist

✅ **CRITICAL Security Requirements:**

1. **TenantId in Database:**
   - All tenant-owned entities have `TenantId` column
   - Foreign keys reference tenant-specific data only

2. **Global Query Filter:**
   - EF Core global query filter automatically adds `WHERE TenantId = @currentTenantId`
   - Applied to all queries automatically

3. **SaveChanges Override:**
   - Auto-set `TenantId` on entity creation
   - Validate `TenantId` matches current tenant

4. **API Authorization:**
   - Every API endpoint validates tenant context
   - TenantId from JWT token (server-side)
   - Never trust client-supplied TenantId

5. **Testing:**
   - Cross-tenant access tests mandatory
   - Verify user from Tenant A cannot access Tenant B data

### 9.3 OWASP Top 10 Mitigation

| Vulnerability | Mitigation |
|---------------|------------|
| **Injection (SQL)** | EF Core parameterized queries, no raw SQL in business logic |
| **Broken Authentication** | JWT tokens, strong password hashing (bcrypt), HTTPS only |
| **Sensitive Data Exposure** | HTTPS, encrypted secrets, no passwords in logs |
| **XML External Entities** | Not applicable (JSON API) |
| **Broken Access Control** | Role-based authorization, tenant isolation |
| **Security Misconfiguration** | Secure defaults, disable debug in production |
| **XSS** | Angular sanitization, Content Security Policy headers |
| **Insecure Deserialization** | DTOs, model validation |
| **Using Components with Known Vulnerabilities** | Regular dependency updates, Dependabot |
| **Insufficient Logging & Monitoring** | Structured logging, exception tracking |

---

## 10. Deployment Architecture

### 10.1 Production Deployment Topology

```
                      ┌──────────────┐
                      │   Internet   │
                      └──────┬───────┘
                             │
                             │ HTTPS (443)
                             │
                      ┌──────▼───────┐
                      │  Nginx/LB    │
                      │  (Reverse    │
                      │   Proxy)     │
                      └──────┬───────┘
                             │
           ┌─────────────────┼─────────────────┐
           │                 │                 │
    ┌──────▼──────┐   ┌──────▼──────┐   ┌──────▼──────┐
    │   Angular   │   │ ASP.NET Core│   │ ASP.NET Core│
    │  Static Web │   │  API (1)    │   │  API (2)    │
    │   (Nginx)   │   │             │   │  (Optional) │
    └─────────────┘   └──────┬──────┘   └──────┬──────┘
                             │                 │
                             └────────┬────────┘
                                      │
                             ┌────────▼────────┐
                             │   PostgreSQL    │
                             │    Database     │
                             └─────────────────┘
```

### 10.2 Docker Compose Setup (Development)

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_USER: inventoryproc
      POSTGRES_PASSWORD: dev_password
      POSTGRES_DB: inventoryproc_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: DevPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=inventoryproc_db;Username=inventoryproc;Password=dev_password"
    depends_on:
      - postgres

  angular:
    build:
      context: ./frontend-angular
      dockerfile: Dockerfile
    ports:
      - "4200:80"
    depends_on:
      - api

volumes:
  postgres_data:
  sqlserver_data:
```

### 10.3 CI/CD Pipeline

```yaml
# GitHub Actions / Azure DevOps Pipeline

name: Build and Deploy

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

      - name: Publish
        run: dotnet publish -c Release -o ./publish

      - name: Build Docker image
        run: docker build -t inventoryproc-api:${{ github.sha }} .

      - name: Push to registry
        run: docker push inventoryproc-api:${{ github.sha }}

  build-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build --prod

      - name: Test
        run: npm run test:ci

      - name: Build Docker image
        run: docker build -t inventoryproc-web:${{ github.sha }} .

  deploy-production:
    needs: [build-backend, build-frontend]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy to production
        run: |
          # Deployment commands here
          # e.g., kubectl apply, docker compose, etc.
```

---

## 11. Future Evolution Path

### 11.1 From Modular Monolith to Microservices

**When to Extract a Module:**
- Independent scaling requirements
- Different deployment cadence needed
- Team organizational boundaries
- Performance isolation required
- Technology diversity justified

**Extraction Process:**

```
Phase 1: Modular Monolith (MVP)
┌────────────────────────────────┐
│      Single Application         │
│   ┌──────────────────────┐     │
│   │  All Modules         │     │
│   │  One Database        │     │
│   └──────────────────────┘     │
└────────────────────────────────┘

Phase 2: Extract High-Value Service
┌────────────────────────────────┐
│      Main Application          │
│   ┌──────────────────────┐     │
│   │  Most Modules        │     │
│   │  Main Database       │     │
│   └──────────────────────┘     │
└────────────────────────────────┘
                │
                │ API Call
                │
┌────────────────▼───────────────┐
│   Reporting Microservice       │
│   ┌──────────────────────┐     │
│   │  Reporting Module    │     │
│   │  Read Replica DB     │     │
│   └──────────────────────┘     │
└────────────────────────────────┘

Phase 3: Full Microservices (if justified)
                ┌───────────────┐
                │  API Gateway  │
                └───────┬───────┘
                        │
        ┌───────────────┼───────────────┐
        │               │               │
┌───────▼──────┐ ┌──────▼──────┐ ┌─────▼──────┐
│ Sales Service│ │Inventory Svc│ │ Other Svcs │
│ + Sales DB   │ │ + Inv DB    │ │ + DBs      │
└──────────────┘ └─────────────┘ └────────────┘
```

### 11.2 Scalability Improvements

**Horizontal Scaling:**
- Load balancer + multiple API instances
- Stateless API design (no in-memory session state)
- Distributed caching (Redis) if needed

**Database Scaling:**
- Read replicas for reporting queries
- Connection pooling optimization
- Query optimization and indexing

**Caching Strategy:**
- In-memory cache for reference data (products, categories)
- Distributed cache (Redis) for session/tenant data
- Cache invalidation strategy

### 11.3 Future Enhancements

**Technical Enhancements:**
- Event-driven architecture (domain events, message bus)
- CQRS (separate read/write models)
- Background job processing (Hangfire/Quartz)
- Real-time updates (SignalR)
- API versioning (v1, v2)
- GraphQL API (optional)

**Business Features (Post-MVP):**
- Multi-warehouse support
- Batch/expiry tracking
- Advanced pricing rules
- Customer-specific pricing
- Sales representatives and routes
- Delivery management
- Advanced GST/tax engine
- Accounting integrations
- Notifications (email, SMS, push)
- Mobile barcode scanning

---

## Appendix A: Design Patterns Used

| Pattern | Usage | Module/Layer |
|---------|-------|--------------|
| **Repository** | Data access abstraction | Infrastructure |
| **Unit of Work** | Transaction management | EF Core DbContext |
| **Dependency Injection** | Loose coupling | All layers |
| **CQRS (lightweight)** | Separate commands/queries | Application layer |
| **Middleware Pipeline** | Cross-cutting concerns | API |
| **Factory** | DbContext creation | Infrastructure |
| **Strategy** | Database provider selection | Infrastructure |
| **Decorator** | Logging, caching (future) | Application services |
| **Observer** | Domain events (future) | Domain layer |

---

## Appendix B: Key Architectural Decisions (ADRs)

### ADR-001: Modular Monolith
**Decision:** Use modular monolith architecture for MVP
**Rationale:** Simple deployment, lower costs, faster development, while maintaining module boundaries for future extraction
**Status:** Accepted

### ADR-002: Database Portability
**Decision:** Support both SQL Server (dev) and PostgreSQL (prod)
**Rationale:** Developer Windows machines use SQL Server; Production uses cost-effective PostgreSQL
**Status:** Accepted

### ADR-003: Single DbContext
**Decision:** Use one EF Core DbContext for all modules initially
**Rationale:** Simplifies transactions and deployment; logical boundaries still enforced through module structure
**Status:** Accepted

### ADR-004: No Generic Repository
**Decision:** Use selective repositories, not generic repository pattern
**Rationale:** Generic repositories add complexity without benefit; EF Core DbSet already provides repository-like interface
**Status:** Accepted

### ADR-005: JWT Authentication
**Decision:** Use JWT tokens for authentication
**Rationale:** Stateless, scalable, standard, works with Angular/Flutter
**Status:** Accepted

---

## Appendix C: Technology Evaluation

### Why Not Node.js?
- Team has .NET expertise
- .NET has excellent business application tooling
- No compelling reason to switch
- .NET performance is excellent

### Why Not React?
- Angular selected for structure and TypeScript integration
- Enterprise-ready with comprehensive tooling
- Team familiar with Angular

### Why Not Microservices from Start?
- MVP doesn't justify distributed complexity
- Modular monolith faster to develop
- Lower infrastructure costs
- Can extract later if needed

---

**Document Status:** This is a living document and will be updated as architectural decisions evolve during implementation.
