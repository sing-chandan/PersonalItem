# Wholesale / Distributor Business Management SaaS
## Phase 1 — Master Product, Architecture & AI Handoff Document

**Status:** Baseline / Architecture Contract  
**Purpose:** This document is the source of truth for producing Phase 2 and then the implementation.

---

## 0. AI HANDOFF — READ FIRST

You are an AI software architect/development agent receiving this document as the agreed Phase 1 direction for a real commercial product.

**Do not immediately start coding.**

First create **Phase 2 — Full Application Specification** from this document.

Rules:

1. Preserve every decision marked LOCKED.
2. Identify assumptions and unresolved decisions instead of silently inventing major behavior.
3. Keep the MVP simple enough for wholesalers/distributors who may be moving from Excel/manual processes.
4. Do not turn the MVP into a full ERP.
5. Design as a multi-tenant SaaS product from day one.
6. Use a modular monolith initially.
7. Keep module boundaries strong enough for future microservice extraction.
8. Use Angular + TypeScript for web.
9. Use Flutter for mobile.
10. Use ASP.NET Core/.NET for backend.
11. Use EF Core.
12. Production database is PostgreSQL.
13. Development must support SQL Server.
14. Use selective repositories/query services; do not create a generic repository over every table.
15. Use one DbContext initially, while enforcing logical module boundaries.
16. Reporting is a first-class capability.
17. Excel import is a key MVP capability.
18. Before implementation, produce detailed requirements, screens, APIs, domain model, database schema, permissions, workflows, reports, import rules, tests, deployment and implementation backlog.
19. Only begin implementation after the detailed specification is internally consistent.

---

## 1. PRODUCT VISION

Build a practical SaaS product for wholesalers/distributors who purchase goods from suppliers and sell them to retail shops.

The system should:

- Reduce manual work.
- Reduce operational cost.
- Reduce duplicate data entry.
- Improve stock visibility.
- Make sales and purchasing easier.
- Track customer payments and outstanding balances.
- Provide useful business-wide reports.
- Allow migration from Excel/manual records.
- Give owners a clear view of the whole business.

The first release is **not a full ERP**. The first objective is:

> Build a credible prototype quickly → demonstrate it to real wholesalers/distributors → collect feedback → refine the workflow → build the production product.

### Core business loop

```text
Business / Tenant
    |
    +-- Products
    +-- Customers / Retail Shops
    +-- Suppliers
    |
    +-- Purchase / Stock In ----> Inventory
    |
    +-- Sales / Billing --------> Inventory
    |
    +-- Payments ---------------> Customer Outstanding
    |
    +-- Reports ----------------> Whole-business visibility
```

---

## 2. TARGET CUSTOMER AND USERS

### Primary customer

A wholesaler/distributor that:

- Buys products from suppliers.
- Keeps stock.
- Sells products to retail shops.
- Often uses Excel, notebooks, or basic software today.
- Needs simple workflows rather than enterprise terminology.

### Initial user types

- Business Owner / Admin.
- Manager.
- Sales Staff.
- Viewer / Read-only user.

### Important UX constraint

Many target users may be new to web applications. The application must therefore feel simple, familiar and business-oriented.

Avoid exposing:

- Technical architecture.
- Complex configuration.
- Unnecessary accounting concepts.
- Complex permissions.
- Microservice concepts.
- Database terminology.

---

## 3. LOCKED TECHNOLOGY STACK

| Area | Decision |
|---|---|
| Web | Angular + TypeScript |
| Mobile | Flutter |
| Backend | ASP.NET Core / .NET |
| ORM | Entity Framework Core |
| Production DB | PostgreSQL |
| Development DB | SQL Server |
| Architecture | Modular Monolith |
| Future architecture | Microservice extraction when justified |
| API | REST/JSON |
| Deployment | Linux/Docker-friendly |

### Why .NET

.NET is selected as the backend because:

- It matches existing development experience.
- It is strong for business applications.
- It works well with EF Core.
- It provides mature authentication, validation, logging and API tooling.
- It supports the modular-monolith-to-services evolution.

Node.js was considered, but the product does not require switching to it for hosting-cost reasons.

---

## 4. ARCHITECTURE — MODULAR MONOLITH

Use one deployable backend initially with strict logical module boundaries.

```text
                    ASP.NET CORE APPLICATION
                              |
        +---------------------+----------------------+
        |                     |                      |
     Products             Customers              Suppliers
        |                     |                      |
        +---------------------+----------------------+
                              |
                    +---------+---------+
                    |                   |
                  Sales             Purchase
                    |                   |
                    +---------+---------+
                              |
                         Inventory
                              |
                         Payments
                              |
                         Reporting

                    ONE DEPLOYMENT
                    ONE DB CONTEXT
                    ONE DATABASE
                 SQL Server / PostgreSQL
```

The goal is **module isolation**, not premature physical separation.

---

## 5. FUTURE MICROSERVICE EXTRACTION

Do not start with full microservices.

Instead, make module boundaries strong enough that later extraction is possible.

```text
MVP:
Client -> API -> Modules -> One DB

Future, only if justified:

Client
  |
API Gateway / Edge
  |
  +-- Sales Service + Sales DB
  +-- Inventory Service + Inventory DB
  +-- Reporting Service + Reporting DB
  +-- Other services when justified
```

A future extraction should move:

- Domain logic.
- Application logic.
- Infrastructure.
- Contracts.
- Data ownership.

together.

Microservices should only be introduced when justified by:

- Scale.
- Independent deployment.
- Team boundaries.
- Operational requirements.
- Reliability/isolation.
- Measured performance requirements.

---

## 6. DATABASE PROVIDER PORTABILITY

A hard requirement:

```text
Development:
Angular / Flutter -> ASP.NET Core -> EF Core -> SQL Server

Production:
Angular / Flutter -> ASP.NET Core -> EF Core -> PostgreSQL
```

Core business logic must not depend on a specific database provider.

### Rules

- Use portable EF Core/LINQ.
- Avoid SQL Server-only SQL in business logic.
- Avoid PostgreSQL-specific behavior in business logic.
- Isolate unavoidable provider-specific code inside Infrastructure.
- Define migrations carefully for both providers.
- Run integration tests against PostgreSQL and SQL Server.
- Do not use provider-specific database types unless there is a documented reason.

---

## 7. MVP MODULES

1. Tenant / Platform.
2. Identity & Access.
3. Dashboard.
4. Products.
5. Customers / Retail Shops.
6. Suppliers.
7. Inventory.
8. Purchase / Stock In.
9. Sales / Billing.
10. Payments / Outstanding.
11. Reports.
12. Excel Import / Export.

Keep the initial scope deliberately small.

---

## 8. OUT OF MVP

Do not build these unless validated:

- Full accounting/general ledger.
- Advanced GST/tax engine.
- Manufacturing.
- Complex approval workflows.
- Batch/expiry.
- User-facing multi-warehouse management.
- Route optimization.
- Advanced delivery management.
- Complex customer-specific pricing engine.
- Multi-currency.
- Advanced subscription billing.
- Large configurable permission designer.

The architecture may be extensible, but the MVP UX must remain simple.

---

## 9. UX PRINCIPLES

- Use familiar terms.
- Minimize clicks.
- Provide sensible defaults.
- Make common actions fast.
- Use clear validation messages.
- Keep business numbers visible.
- Make Excel import understandable.
- Avoid unnecessary charts.
- Optimize desktop web for daily business operations.
- Use Flutter for focused mobile workflows.

Example quick actions:

```text
New Sale
Add Purchase
Add Customer
Add Product
Record Payment
Import Excel
```

---

## 10. MULTI-TENANCY

Each customer business is a Tenant.

Example:

```text
Tenant
  Id
  Name
  Code
  BusinessName
  Email
  Phone
  Address
  GSTNumber
  IsActive
  CreatedAt
  UpdatedAt
```

### Rules

- Tenant isolation is a critical security boundary.
- Tenant-owned entities contain TenantId unless genuinely global.
- Tenant context comes from authenticated identity/session/token.
- Never trust a client-supplied TenantId for authorization.
- Every tenant query must be tenant-safe.
- Unique constraints should consider TenantId where appropriate.
- Cross-tenant access tests are mandatory.

---

## 11. INITIAL ROLES

### Admin / Owner

Full business access.

### Manager

Operational modules and reports.

### Sales Staff

Customer/product lookup and sales workflows according to permissions.

### Viewer

Dashboard and reports read-only.

Do not build a complex permission designer for MVP.

---

## 12. INITIAL DOMAIN MODEL

### Product

```text
Product
  Id
  TenantId
  Name
  SKU
  Barcode
  CategoryId
  BrandId
  UnitId
  PurchasePrice
  SellingPrice
  MinimumStock
  IsActive
```

### Customer / Retail Shop

```text
Customer
  Id
  TenantId
  ShopName
  OwnerName
  Mobile
  Email
  Address
  City
  State
  Pincode
  GSTNumber
  CreditLimit
  CreditDays
  OpeningBalance
  IsActive
```

### Supplier

```text
Supplier
  Id
  TenantId
  Name
  ContactPerson
  Mobile
  Email
  Address
  City
  State
  Pincode
  GSTNumber
  OpeningBalance
  IsActive
```

### Purchase

```text
Purchase
  Id
  TenantId
  SupplierId
  InvoiceNumber
  PurchaseDate
  SubTotal
  Discount
  Tax
  GrandTotal
  Notes
  CreatedAt
  CreatedBy

PurchaseItem
  Id
  PurchaseId
  ProductId
  Quantity
  UnitPrice
  Discount
  Tax
  Total
```

### Sale

```text
Sale
  Id
  TenantId
  CustomerId
  InvoiceNumber
  SaleDate
  SubTotal
  Discount
  Tax
  GrandTotal
  PaidAmount
  DueAmount
  PaymentStatus
  Notes
  CreatedAt
  CreatedBy

SaleItem
  Id
  SaleId
  ProductId
  Quantity
  UnitPrice
  Discount
  Tax
  Total
```

### Inventory

```text
StockBalance
  Id
  TenantId
  ProductId
  WarehouseId
  QuantityOnHand
  ReservedQuantity
  UpdatedAt

InventoryTransaction
  Id
  TenantId
  ProductId
  WarehouseId
  TransactionType
  Quantity
  ReferenceType
  ReferenceId
  TransactionDate
  CreatedBy
  CreatedAt
```

Initial transaction types:

- OpeningStock.
- Purchase.
- Sale.
- Adjustment.

Future:

- Return.
- TransferIn.
- TransferOut.

### Payment

```text
Payment
  Id
  TenantId
  CustomerId
  PaymentDate
  Amount
  PaymentMethod
  ReferenceNumber
  Notes
  CreatedBy
  CreatedAt
```

### Customer Ledger

```text
CustomerLedgerEntry
  Id
  TenantId
  CustomerId
  EntryDate
  EntryType
  ReferenceType
  ReferenceId
  Debit
  Credit
  Notes
```

---

## 13. INVENTORY RULES

Inventory belongs to the Inventory module.

Sales and Purchase must call Inventory contracts/services instead of directly updating inventory tables.

```text
Purchase
  -> Inventory contract
  -> Increase stock
  -> Supplier payable/ledger

Sale
  -> Validate stock
  -> Inventory contract
  -> Decrease stock
  -> Customer ledger
  -> Payment if supplied
```

### Important

A business transaction that changes multiple records must be atomic where possible.

Example sale:

```text
Validate
  |
Create Sale
  |
Create SaleItems
  |
Decrease Stock
  |
Create Customer Ledger
  |
Create Payment (if any)
  |
Commit
```

If a required operation fails, the transaction should roll back.

---

## 14. REPOSITORY + DBCONTEXT

Use one DbContext initially.

EF Core DbContext already provides Unit-of-Work behavior.

Do not create a generic repository over every table.

```text
Application
  Commands / Queries / Services
            |
            v
Infrastructure
  Selective Repositories
  Query Services
            |
            v
EF Core DbContext
            |
            v
SQL Server / PostgreSQL
```

### Rules

- Repository for meaningful aggregates/persistence where useful.
- Query services/projections for reporting.
- DTOs at API boundaries.
- Transaction boundary at application use case.
- Modules cannot call another module's repository directly.
- One DbContext is an implementation convenience, not permission to bypass boundaries.

---

## 15. MODULE COMMUNICATION

Example:

```text
Sales Application
   |
   +--> ISalesRepository
   +--> IInventoryService
   +--> ICustomerService
   +--> IPaymentService
```

Sales must not:

- Use InventoryRepository directly.
- Update StockBalance directly.
- Access another module's private DbSet.
- Depend on another module's Infrastructure implementation.

This allows:

```text
Today:
Sales -> IInventoryService -> internal Inventory implementation

Future:
Sales Service -> Inventory API/message contract
```

---

## 16. REPORTING

Reporting is a first-class module.

```text
Client
  |
Reporting API
  |
Reporting Query Services
  |
Optimized projections
  |
Database
```

Angular/Flutter should not be responsible for authoritative business calculations.

### Initial reports

- Dashboard / Business Summary.
- Sales Summary.
- Sales by Product.
- Sales by Customer.
- Purchase Summary.
- Purchase by Supplier.
- Current Stock.
- Stock Movement.
- Low Stock.
- Customer Outstanding.
- Customer Statement.
- Payment History.
- Top Selling Products.
- Slow Moving Products.
- Whole-business summary.

Common filters:

- From Date.
- To Date.
- Customer.
- Product.
- Category.
- Supplier.
- Status.

---

## 17. EXCEL IMPORT

Initial imports:

- Products.
- Customers.
- Suppliers.
- Opening Stock.

Workflow:

```text
Upload
  |
Validate file
  |
Validate columns
  |
Validate rows
  |
Preview
  |
Confirm
  |
Import
  |
Result:
Imported
Skipped
Errors
```

Do not silently import invalid rows.

---

## 18. API PRINCIPLES

- REST/JSON.
- Consistent routes.
- DTOs.
- Server-side validation.
- Consistent errors.
- Pagination.
- Filtering.
- Sorting.
- Tenant authorization.
- Server-side calculation of authoritative totals.
- Concurrency handling where required.
- API versioning strategy before external integrations.

Example:

```text
GET    /api/products
POST   /api/products
GET    /api/products/{id}
PUT    /api/products/{id}

GET    /api/customers
POST   /api/customers

POST   /api/sales
GET    /api/sales/{id}

POST   /api/purchases
GET    /api/purchases/{id}

POST   /api/payments

GET    /api/inventory/stock
GET    /api/inventory/movements

GET    /api/reports/dashboard
GET    /api/reports/sales/summary
GET    /api/reports/customers/outstanding
```

These are examples; Phase 2 must define the actual API contract.

---

## 19. ANGULAR STRUCTURE

```text
frontend-angular/
  src/
    app/
      core/
        auth/
        http/
        guards/
        interceptors/
        tenant/
        layout/
      shared/
        components/
        directives/
        pipes/
        models/
      features/
        dashboard/
        products/
        customers/
        suppliers/
        inventory/
        purchases/
        sales/
        payments/
        reports/
        imports/
      app.routes.ts
    assets/
    environments/
```

Use modern Angular conventions and standalone components where appropriate.

---

## 20. FLUTTER STRUCTURE

```text
mobile-flutter/
  lib/
    core/
      networking/
      auth/
      storage/
      routing/
      theme/
    shared/
      widgets/
      models/
    features/
      dashboard/
      customers/
      products/
      sales/
      payments/
      outstanding/
      reports/
```

Flutter uses the same backend API.

Initial mobile focus should be high-value workflows, not every web screen.

---

## 21. .NET STRUCTURE

```text
backend/
  src/
    Host/
      API/
    BuildingBlocks/
      SharedKernel/
      Contracts/
      Infrastructure/
    Modules/
      Tenancy/
        Domain/
        Application/
        Infrastructure/
        Api/
      Identity/
        Domain/
        Application/
        Infrastructure/
        Api/
      Products/
        Domain/
        Application/
        Infrastructure/
        Api/
      Customers/
      Suppliers/
      Inventory/
      Purchases/
      Sales/
      Payments/
      Reporting/
  tests/
    UnitTests/
    IntegrationTests/
    ApiTests/
```

The exact physical number of projects can be reduced for prototype speed, but logical ownership must remain clear.

---

## 22. DEPLOYMENT

Recommended initial production topology:

```text
Internet
   |
TLS Reverse Proxy
   |
   +--> Angular static application
   |
   +--> ASP.NET Core API
            |
            +--> PostgreSQL
            +--> optional worker later
            +--> optional object storage later
```

Recommended:

- Linux.
- Docker.
- HTTPS.
- Backups.
- Restore testing.
- Health checks.
- Logging.
- Monitoring.
- Secrets management.
- Rollback procedure.

Do not add Redis/message brokers/read replicas unless actual requirements justify them.

A dedicated server can be used when customer/load requirements justify it. Initial prototype infrastructure should remain inexpensive.

---

## 23. SECURITY

Mandatory:

- Tenant isolation.
- Strong password hashing.
- HTTPS.
- Server-side authorization.
- No trust in client TenantId.
- No trust in client totals.
- No trust in client stock availability.
- File upload validation.
- Secret management.
- Safe logging.
- Abuse protection.
- Audit of sensitive operations.

---

## 24. PERFORMANCE

- Tenant-aware indexes.
- Pagination.
- Efficient projections.
- Avoid N+1 queries.
- Async operations.
- Optimize reports separately from transactional workflows.
- Measure before introducing caching/distributed infrastructure.

---

## 25. TESTING

Required areas:

- Domain/unit tests.
- Application/use-case tests.
- API tests.
- Tenant-isolation tests.
- PostgreSQL integration tests.
- SQL Server compatibility tests.
- Import validation tests.
- Transaction rollback tests.
- End-to-end tests for core business journeys.

---

## 26. DEMO DATA

Create a demo tenant containing:

- 20–50 products.
- 10–20 retail customers.
- 3–5 suppliers.
- Opening stock.
- Purchases.
- Sales.
- Payments.
- Outstanding balances.
- Enough historical data for useful reports.

The demo should let a prospect understand the product in minutes.

---

## 27. FUTURE EXTENSIONS

Potential future modules/features:

- Multiple warehouses.
- Returns.
- Batch/expiry.
- Advanced pricing/schemes.
- Sales representatives.
- Routes.
- Delivery.
- Barcode scanning.
- Customer-specific pricing.
- Advanced GST/accounting integrations.
- Background imports.
- Notifications.
- Separate reporting database.
- Microservices.
- SaaS subscription/feature management.

---

## 28. ADDITIONAL RECOMMENDATIONS

Recommended low-complexity additions:

- Global search.
- Keyboard-friendly sales entry.
- Invoice numbering strategy.
- Tenant business timezone.
- Centralized rounding/number rules.
- Product minimum-stock configuration.
- Excel/CSV report exports.
- Printable sales invoice.
- Basic activity history.
- Deactivate/archive master data rather than deleting it.
- Concurrency protection.
- Duplicate-request protection for sensitive operations.
- Feature flags/tenant configuration for controlled rollout.

---

## 29. PHASE 2 REQUIRED CONTENT

Phase 2 must define:

- Functional requirements.
- User personas.
- Role/permission matrix.
- User journeys.
- Acceptance criteria.
- Angular screens.
- Flutter screens.
- Navigation.
- Complete API catalog.
- Request/response DTOs.
- Validation.
- Error model.
- Domain model.
- ER diagram.
- Database schema.
- Keys/constraints/indexes.
- Tenant strategy.
- EF Core configuration.
- SQL Server/PostgreSQL migration strategy.
- Transaction boundaries.
- Concurrency.
- Module dependency graph.
- Internal contracts.
- Reporting query specifications.
- Excel templates and validation.
- Invoice requirements.
- Audit rules.
- Security model.
- Testing matrix.
- Deployment.
- CI/CD.
- Backup/DR.
- Logging/monitoring.
- Demo data.
- Implementation backlog.
- Definition of Done.
- Risks and open decisions.

---

## 30. IMPLEMENTATION ORDER

1. Architecture skeleton.
2. Tenant + identity.
3. Product/customer/supplier masters.
4. Opening stock/inventory.
5. Purchase.
6. Sales.
7. Payments/outstanding.
8. Dashboard.
9. Reports.
10. Excel imports.
11. Invoice print/export.
12. Mobile MVP.
13. Security hardening.
14. Provider compatibility testing.
15. Production deployment.
16. Real-customer feedback.

Prioritize a complete end-to-end business loop over broad unfinished features.

---

## 31. MVP DEFINITION OF DONE

The MVP is complete when:

- A tenant can securely log in.
- Tenant data is isolated.
- Masters can be managed/imported.
- Opening stock can be loaded.
- Purchase updates stock.
- Sale updates stock.
- Payment updates outstanding.
- Dashboard shows authoritative numbers.
- Core reports work.
- Excel import validates and reports errors.
- Angular UI is usable by non-technical users.
- Flutter completes agreed high-value workflows.
- APIs are documented/tested.
- PostgreSQL production deployment works.
- SQL Server development compatibility is verified.
- Backups/restore are tested.
- Security and transaction tests pass.
- Demo data is available.

---

## 32. FINAL LOCKED CONTRACT

```text
Web              = Angular + TypeScript
Mobile            = Flutter
Backend           = ASP.NET Core / .NET
ORM               = EF Core
Production DB     = PostgreSQL
Development DB    = SQL Server
Architecture      = Modular Monolith
Future             = Extractable Microservices if justified
API               = Shared REST/JSON
Persistence       = DbContext + selective repositories/query services
DbContext          = One initially
Tenancy            = Multi-tenant from day one
Import             = Excel-first
Reporting          = First-class module
UX                 = Simple wholesaler-friendly
Goal               = Prototype -> Demo -> Feedback -> Refine -> Production
```

### Do not

- Start with full microservices.
- Build a full ERP.
- Use Ionic.
- Replace .NET with Node.js.
- Couple business logic to SQL Server.
- Make Angular authoritative for business calculations.
- Use generic repository everywhere.
- Let modules bypass their contracts.
- Trust client tenant/financial/stock values.
- Delete posted transaction history casually.
- Add infrastructure without a measured need.
- Build large features before validating the real workflow.
