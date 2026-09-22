# Wholesale / Distributor Business Management SaaS
## Phase 2 — Full Application Specification & Implementation Blueprint

**Document status:** Detailed specification derived from Phase 1  
**Audience:** Product owner, architect, developers, QA, DevOps, Codex/AI coding agents  
**Primary goal:** This document is intended to become the implementation blueprint. Any decision not explicitly locked here must be treated as a documented assumption or open product decision.

---

# 1. HOW THE DEVELOPMENT AI MUST USE THIS DOCUMENT

## 1.1 Execution rules

Before writing production code:

1. Read Phase 1 and this Phase 2 specification.
2. Do not change locked technology/architecture decisions without explicit approval.
3. If an ambiguity affects data integrity, tenant isolation, API compatibility, or financial/stock correctness, stop and raise the decision.
4. If an ambiguity is low-risk, use the documented default assumption and record it.
5. Implement vertically by business workflow rather than creating empty layers for every module.
6. Every completed feature must have tests.
7. Do not expose EF Core entities directly from APIs.
8. Do not let one module access another module's Infrastructure/persistence.
9. Keep provider-neutral business logic for SQL Server/PostgreSQL.
10. Keep the application deployable as one modular monolith.

## 1.2 Required implementation sequence

```text
Foundation
  -> Tenant
  -> Identity
  -> Products
  -> Customers
  -> Suppliers
  -> Inventory
  -> Purchase
  -> Sales
  -> Payments
  -> Reports
  -> Import
  -> Invoice
  -> Flutter MVP
  -> Production hardening
```

---

# 2. PRODUCT SCOPE

## 2.1 MVP outcome

A wholesaler should be able to:

1. Create/use their business.
2. Log in.
3. Add or import products.
4. Add or import retail shops.
5. Add or import suppliers.
6. Add opening stock.
7. Record purchases.
8. See stock increase.
9. Make sales.
10. See stock decrease.
11. Record customer payments.
12. See outstanding balances.
13. View customer statements.
14. View business dashboard.
15. Run core reports.
16. Import Excel data safely.
17. Print/download an invoice.
18. Use selected mobile workflows.

## 2.2 Non-goals

Do not make the MVP a general accounting package or full ERP.

---

# 3. PRODUCT PERSONAS

| Persona | Goal | Main actions |
|---|---|---|
| Owner/Admin | Understand business and control data | Dashboard, masters, sales, purchases, payments, reports, users |
| Manager | Run daily operations | Sales, purchases, stock, customers, suppliers, reports |
| Sales Staff | Quickly create sales | Customer lookup, product lookup, sales, payment |
| Viewer | See business state | Dashboard, reports |
| Future Field Sales | Work from phone | Customers, products, sales/order, outstanding |

---

# 4. ROLE/PERMISSION MATRIX

Initial permission model should be simple.

| Capability | Admin | Manager | Sales | Viewer |
|---|---:|---:|---:|---:|
| Dashboard | Yes | Yes | Limited | Yes |
| Products view | Yes | Yes | Yes | No/limited |
| Products edit | Yes | Yes | No | No |
| Customers view | Yes | Yes | Yes | No/limited |
| Customers edit | Yes | Yes | Yes/limited | No |
| Suppliers | Yes | Yes | No | No |
| Purchase | Yes | Yes | No/limited | No |
| Sales | Yes | Yes | Yes | No |
| Payments | Yes | Yes | Yes/limited | No |
| Inventory | Yes | Yes | View | View |
| Reports | Yes | Yes | Selected | Yes |
| Import | Yes | Yes | No | No |
| Users | Yes | No/limited | No | No |
| Tenant settings | Yes | No | No | No |

The exact policy names must be defined in code, for example:

```text
products.read
products.write
customers.read
customers.write
sales.create
sales.read
sales.void
purchases.create
inventory.read
inventory.adjust
payments.create
reports.read
imports.execute
users.manage
tenant.manage
```

---

# 5. UX / INFORMATION ARCHITECTURE

## 5.1 Primary navigation

Recommended web navigation:

```text
Dashboard

Masters
  Products
  Customers
  Suppliers

Transactions
  Sales
  Purchases
  Payments

Inventory
  Stock
  Stock Movements
  Adjustments

Reports
  Sales
  Purchases
  Inventory
  Customers / Outstanding
  Business Summary

Import
  Excel Import

Administration
  Users
  Business Settings
```

Keep navigation collapsible.

## 5.2 Global utilities

- Global search.
- Notifications/errors.
- User menu.
- Business/tenant identity.
- Date/filter context where appropriate.
- Help/first-use hints later.

---

# 6. FIRST-USE ONBOARDING

The first-time business owner should see a short setup flow:

```text
Welcome
  |
Business Details
  |
Add/Import Products
  |
Add/Import Customers
  |
Add/Import Suppliers
  |
Opening Stock
  |
Ready
```

Allow skipping optional steps and returning later.

Do not force a complicated setup wizard.

---

# 7. SCREEN SPECIFICATION

## 7.1 Dashboard

### Purpose

Answer:

- How is the business doing?
- How much did we sell?
- How much did we purchase?
- How much have customers paid?
- How much is outstanding?
- How much stock is available?
- Which products are low?

### Components

```text
Today's Sales
Today's Purchases
Today's Receipts
Total Outstanding
Stock Value
Low Stock Count
```

### Additional sections

- Recent sales.
- Recent payments.
- Low-stock products.
- Sales trend.
- Outstanding customers.

### Quick actions

- New Sale.
- Add Purchase.
- Add Customer.
- Add Product.
- Record Payment.
- Import Excel.

---

# 8. PRODUCT MODULE

## 8.1 Product list

Columns:

- Product name.
- SKU.
- Barcode.
- Category.
- Brand.
- Unit.
- Purchase price.
- Selling price.
- Current stock.
- Minimum stock.
- Active status.

Features:

- Search.
- Filter.
- Sort.
- Pagination.
- Add.
- Edit.
- Activate/deactivate.
- Export.

## 8.2 Product form

Required minimum:

- Name.
- SKU.
- Unit.
- Selling price.

Recommended:

- Barcode.
- Category.
- Brand.
- Purchase price.
- Minimum stock.

Validation:

- Name required.
- SKU unique within tenant.
- Price >= 0.
- Minimum stock >= 0.
- Barcode unique within tenant if supplied.

---

# 9. CUSTOMER MODULE

## 9.1 Customer list

Show:

- Shop name.
- Owner.
- Mobile.
- City.
- Outstanding.
- Credit limit.
- Status.

Actions:

- View.
- Edit.
- New sale.
- Record payment.
- Statement.

## 9.2 Customer detail

Tabs:

```text
Overview
Sales
Payments
Statement
Outstanding
```

Summary:

- Total sales.
- Total paid.
- Current outstanding.
- Credit limit.
- Last transaction.

---

# 10. SUPPLIER MODULE

List:

- Name.
- Contact.
- Mobile.
- City.
- Outstanding/payable if implemented.
- Status.

Actions:

- View.
- Edit.
- Purchase.
- Statement later.

---

# 11. SALES MODULE

## 11.1 New sale UX

The sales screen should be one of the fastest screens.

```text
Customer
   |
Product Search
   |
Add Product
   |
Quantity
   |
Price
   |
Discount
   |
Line Total
   |
Summary
   |
Paid
   |
Due
   |
Save / Print
```

## 11.2 Sale validation

Server must validate:

- Tenant.
- Customer.
- Product.
- Quantity > 0.
- Price >= 0.
- Discount rules.
- Tax rules if enabled.
- Stock availability.
- Total calculations.
- Payment amount <= allowed amount.

The server recalculates totals.

## 11.3 Sale transaction

Use one application transaction:

```text
Validate command
  |
Load customer/products
  |
Check stock
  |
Calculate authoritative totals
  |
Create sale
  |
Create items
  |
Inventory decrease
  |
Ledger debit
  |
Payment/receipt if present
  |
Commit
```

If any required operation fails, rollback.

## 11.4 Sale cancellation

Do not physically delete posted sales.

Recommended future-safe approach:

```text
Posted
  |
Void / Cancel
  |
Reverse inventory
  |
Reverse ledger
  |
Audit reason
```

Phase 2 default: define cancellation before production release.

---

# 12. PURCHASE MODULE

## 12.1 Purchase screen

```text
Supplier
  |
Supplier Invoice No
  |
Date
  |
Product Search
  |
Quantity
  |
Purchase Price
  |
Discount/Tax
  |
Total
  |
Save
```

On save:

```text
Purchase
  -> Inventory increase
  -> Supplier payable if enabled
```

---

# 13. INVENTORY MODULE

## 13.1 Stock screen

Columns:

- Product.
- SKU.
- Current stock.
- Reserved stock.
- Available stock.
- Minimum stock.
- Status.

Statuses:

```text
Healthy
Low Stock
Out of Stock
```

## 13.2 Stock movement screen

Filters:

- Product.
- Date range.
- Transaction type.
- Reference.

Columns:

- Date.
- Product.
- Type.
- Quantity in/out.
- Reference.
- User.

## 13.3 Adjustment

Adjustment must require:

- Product.
- Current stock.
- Adjustment quantity.
- Direction.
- Reason.

Every adjustment creates an InventoryTransaction.

---

# 14. PAYMENT / OUTSTANDING

## 14.1 Record payment

Fields:

- Customer.
- Date.
- Amount.
- Payment method.
- Reference number.
- Notes.

Methods:

- Cash.
- UPI.
- Bank.
- Cheque.
- Other.

## 14.2 Outstanding screen

Columns:

- Customer.
- Total sales/receivable.
- Paid.
- Outstanding.
- Credit limit.
- Days outstanding.

Filters:

- Date.
- Customer.
- Outstanding > 0.

## 14.3 Customer statement

Show chronological ledger:

```text
Date | Reference | Debit | Credit | Balance
```

Opening balance must be clearly identified.

---

# 15. REPORTING SPECIFICATION

## 15.1 General report contract

Reports should support where applicable:

```text
FromDate
ToDate
CustomerId
SupplierId
ProductId
CategoryId
Page
PageSize
Sort
```

The API returns:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 25,
  "totalCount": 0,
  "summary": {}
}
```

Exact response envelope can be standardized globally.

## 15.2 Sales summary

Fields:

- Total sales.
- Number of invoices.
- Total quantity.
- Average invoice value.
- Paid.
- Outstanding.

Breakdown:

- Daily.
- Weekly.
- Monthly.

## 15.3 Sales by product

Fields:

- Product.
- Quantity sold.
- Sales amount.
- Discount.
- Net sales.

## 15.4 Sales by customer

Fields:

- Customer.
- Invoice count.
- Sales.
- Paid.
- Outstanding.

## 15.5 Purchase summary

Fields:

- Purchase count.
- Total purchase.
- Quantity.
- Supplier breakdown.

## 15.6 Current stock

Fields:

- Product.
- Current quantity.
- Purchase value.
- Estimated selling value.
- Minimum stock.
- Stock status.

Stock valuation method must be finalized in Phase 2 decision list. MVP should avoid pretending that an advanced accounting valuation method exists if it does not.

## 15.7 Low stock

Products where:

```text
QuantityOnHand <= MinimumStock
```

## 15.8 Outstanding

Fields:

- Customer.
- Opening balance.
- Sales/debit.
- Payments/credit.
- Current balance.

## 15.9 Business summary

The owner-facing report combines:

```text
Sales
Purchases
Receipts
Outstanding
Stock
Top Products
Top Customers
Low Stock
```

---

# 16. EXCEL IMPORT SPECIFICATION

## 16.1 Import framework

Create a reusable internal interface/concept:

```text
IImportHandler<T>
    ValidateStructure()
    ValidateRows()
    Preview()
    Execute()
```

Actual implementation may use a different naming scheme.

## 16.2 Product template

Initial columns:

```text
Name
SKU
Barcode
Category
Brand
Unit
PurchasePrice
SellingPrice
MinimumStock
```

Required:

```text
Name
SKU
Unit
SellingPrice
```

## 16.3 Customer template

```text
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
```

## 16.4 Supplier template

```text
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
```

## 16.5 Opening stock template

```text
SKU
ProductName
Quantity
```

SKU should be the primary matching key where possible.

## 16.6 Import behavior

For every row:

```text
Valid
Warning
Error
```

Example:

```text
Row 17:
ERROR: SKU 'ABC-100' already exists.
```

The user must see a preview before committing.

## 16.7 Duplicate rules

Default recommendation:

- Existing matching SKU -> error/update option must be explicit.
- Existing customer by configured matching strategy -> warning/error.
- Never silently overwrite existing master data.

Phase 2 should finalize matching rules.

---

# 17. INVOICE / DOCUMENT OUTPUT

MVP should support a simple printable invoice.

Contents:

- Business name/details.
- Invoice number.
- Date.
- Customer.
- Items.
- Quantity.
- Unit price.
- Discount.
- Tax if enabled.
- Total.
- Paid.
- Due.
- Notes.

Do not build an advanced invoice designer initially.

---

# 18. DOMAIN RULES

## 18.1 Money

Use decimal types appropriate for EF Core/provider compatibility.

Never use floating-point types for financial amounts.

Centralize rounding rules.

## 18.2 Quantity

Quantity precision must be configurable based on business requirements. Do not hardcode integer-only behavior if products may be sold by weight/volume.

## 18.3 Invoice number

Initial recommendation:

```text
Tenant-specific sequence
```

Example:

```text
INV-000001
INV-000002
```

Phase 2 must define:

- Prefix.
- Reset policy.
- Concurrency.
- Voided numbers.
- Import behavior.

## 18.4 Dates/time

Store timestamps consistently, preferably UTC at infrastructure level, and apply tenant/business timezone for display and business-date reporting.

Tenant should have a business timezone setting.

---

# 19. DATABASE DESIGN

## 19.1 General conventions

Recommended:

- UUID/GUID or provider-neutral key strategy.
- Explicit primary keys.
- Foreign keys.
- Indexes based on query patterns.
- Tenant-aware uniqueness.
- CreatedAt/UpdatedAt.
- CreatedBy/UpdatedBy where meaningful.

The exact key type must be finalized before migrations are generated.

## 19.2 Important indexes

Examples:

```text
Product:
  (TenantId, SKU)
  (TenantId, Barcode)

Customer:
  (TenantId, Mobile)
  (TenantId, ShopName)

Sale:
  (TenantId, SaleDate)
  (TenantId, CustomerId)
  (TenantId, InvoiceNumber)

Purchase:
  (TenantId, PurchaseDate)
  (TenantId, SupplierId)

InventoryTransaction:
  (TenantId, ProductId, TransactionDate)

Payment:
  (TenantId, CustomerId, PaymentDate)
```

Do not create every possible index. Validate with query plans.

---

# 20. EF CORE DESIGN

## 20.1 DbContext

Initial architecture:

```text
Application
     |
Module contracts
     |
Infrastructure
     |
AppDbContext
```

One DbContext initially.

## 20.2 Entity configurations

Use separate configurations rather than putting all configuration inside entity classes.

Example:

```text
Configurations/
  ProductConfiguration
  CustomerConfiguration
  SaleConfiguration
  SaleItemConfiguration
  ...
```

## 20.3 Migrations

Phase 2 default:

- Maintain migrations intentionally.
- Validate migrations against PostgreSQL.
- Maintain a SQL Server-compatible migration path for development.
- Do not assume one generated migration is automatically perfect for both providers.
- If provider-specific migration sets are necessary, keep them isolated and documented.

---

# 21. APPLICATION LAYERS

Each module conceptually has:

```text
Domain
  Entities
  Value Objects
  Rules
  Domain Events
  Contracts

Application
  Commands
  Queries
  DTOs
  Validators
  Services

Infrastructure
  Persistence
  EF Configurations
  Repositories
  Query implementations

API
  Endpoints
  Request/Response contracts
  Authorization
```

Do not create empty abstractions just for architecture diagrams.

---

# 22. COMMAND / QUERY STYLE

Recommended separation:

```text
Commands:
  CreateProduct
  UpdateProduct
  CreateSale
  CreatePurchase
  RecordPayment
  AdjustStock

Queries:
  GetProducts
  GetCustomer
  GetStock
  GetOutstanding
  GetSalesReport
```

The exact CQRS implementation may remain lightweight. Do not introduce a complex messaging framework just to call it CQRS.

---

# 23. VALIDATION

Use server-side validators.

Examples:

### Create Sale

```text
Customer exists
Customer belongs to tenant
Product exists
Product belongs to tenant
Quantity > 0
Price >= 0
Stock sufficient
Discount valid
Payment valid
```

### Create Purchase

```text
Supplier exists
Supplier belongs to tenant
Products valid
Quantity > 0
Price >= 0
Totals valid
```

---

# 24. ERROR MODEL

Use one consistent API error format.

Example:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "Please correct the highlighted fields.",
  "errors": [
    {
      "field": "quantity",
      "code": "INVALID_VALUE",
      "message": "Quantity must be greater than zero."
    }
  ],
  "traceId": "..."
}
```

Common codes:

```text
VALIDATION_ERROR
UNAUTHORIZED
FORBIDDEN
NOT_FOUND
CONFLICT
TENANT_ACCESS_DENIED
INSUFFICIENT_STOCK
DUPLICATE_RECORD
CONCURRENCY_CONFLICT
IMPORT_ERROR
INTERNAL_ERROR
```

Do not expose stack traces to clients in production.

---

# 25. CONCURRENCY

Inventory and invoice numbering require concurrency consideration.

Example:

Two users attempt to sell the last 5 units simultaneously.

The system must prevent both transactions from incorrectly consuming the same stock.

Phase 2 implementation options:

- Optimistic concurrency.
- Atomic database update.
- Transaction + row/version checks.
- Appropriate locking where justified.

Choose the simplest reliable mechanism.

---

# 26. TENANT SECURITY DESIGN

Every request should have a trusted tenant context.

Conceptually:

```text
Authentication
   |
Resolve User
   |
Resolve Tenant
   |
Create TenantContext
   |
Application use case
   |
Tenant-scoped data access
```

Tests must prove:

```text
Tenant A user cannot:
  read Tenant B products
  read Tenant B customers
  read Tenant B sales
  modify Tenant B stock
  modify Tenant B payments
  access Tenant B reports
```

---

# 27. AUTHENTICATION

Recommended:

- Access token/JWT for API access.
- Secure password hashing.
- Refresh tokens if needed.
- Token expiration.
- Revocation strategy.
- Role/policy authorization.

Angular and Flutter must not implement authorization themselves as a security boundary.

The backend is authoritative.

---

# 28. ANGULAR ARCHITECTURE

## 28.1 Core

```text
AuthService
Token handling
HTTP interceptors
Tenant/user context
Route guards
Global error handling
Layout
```

## 28.2 Feature structure

Each feature should own:

```text
pages/
components/
services/
models/
validators/
routes/
```

Avoid a giant shared service that knows every business module.

## 28.3 State management

Do not introduce a heavyweight global state framework unless needed.

Start with feature-local state and services.

Add a state library only when measurable complexity warrants it.

---

# 29. FLUTTER ARCHITECTURE

Use feature-based organization.

Recommended conceptual layers:

```text
Presentation
  |
Application/state
  |
Domain/model where useful
  |
Data/API
```

Do not duplicate backend business rules.

Mobile should consume APIs and handle mobile-specific UI/state.

---

# 30. MOBILE MVP

Initial high-value mobile screens:

1. Login.
2. Dashboard.
3. Customers.
4. Customer detail/outstanding.
5. Products/search.
6. New Sale / order workflow.
7. Payments.
8. Selected reports.

Mobile should not initially duplicate complex admin/import configuration.

---

# 31. API CATALOG — INITIAL SET

The Phase 2 implementation must expand this into a complete endpoint contract.

## Authentication

```text
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

## Tenant

```text
GET  /api/tenant
PUT  /api/tenant
```

## Users

```text
GET  /api/users
POST /api/users
PUT  /api/users/{id}
```

## Products

```text
GET    /api/products
POST   /api/products
GET    /api/products/{id}
PUT    /api/products/{id}
DELETE /api/products/{id}   # only if safe; prefer deactivate
```

## Customers

```text
GET  /api/customers
POST /api/customers
GET  /api/customers/{id}
PUT  /api/customers/{id}
GET  /api/customers/{id}/statement
GET  /api/customers/{id}/outstanding
```

## Suppliers

```text
GET  /api/suppliers
POST /api/suppliers
GET  /api/suppliers/{id}
PUT  /api/suppliers/{id}
```

## Sales

```text
GET  /api/sales
POST /api/sales
GET  /api/sales/{id}
POST /api/sales/{id}/void
```

## Purchases

```text
GET  /api/purchases
POST /api/purchases
GET  /api/purchases/{id}
POST /api/purchases/{id}/void
```

## Inventory

```text
GET  /api/inventory/stock
GET  /api/inventory/movements
POST /api/inventory/adjustments
```

## Payments

```text
GET  /api/payments
POST /api/payments
```

## Reports

```text
GET /api/reports/dashboard
GET /api/reports/sales/summary
GET /api/reports/sales/by-product
GET /api/reports/sales/by-customer
GET /api/reports/purchases/summary
GET /api/reports/purchases/by-supplier
GET /api/reports/inventory/current-stock
GET /api/reports/inventory/low-stock
GET /api/reports/inventory/movements
GET /api/reports/customers/outstanding
GET /api/reports/customers/{id}/statement
GET /api/reports/payments
GET /api/reports/business-summary
```

## Imports

```text
POST /api/imports/products/preview
POST /api/imports/products/execute

POST /api/imports/customers/preview
POST /api/imports/customers/execute

POST /api/imports/suppliers/preview
POST /api/imports/suppliers/execute

POST /api/imports/opening-stock/preview
POST /api/imports/opening-stock/execute
```

---

# 32. API VERSIONING

Recommended initial strategy:

```text
/api/v1/...
```

Do not expose unversioned public APIs if external integrations are expected later.

Internal APIs can remain simple until service extraction.

---

# 33. REPORTING DATA MODEL STRATEGY

Do not create a separate reporting database in MVP.

Use optimized read queries/projections.

Later:

```text
Transactional DB
       |
Events / ETL
       |
Reporting DB
       |
Analytics
```

This is an extraction/scale option, not an MVP requirement.

---

# 34. AUDIT MODEL

Important actions should record:

```text
Who
When
Tenant
Action
Entity
EntityId
Reference
Reason where applicable
```

Examples:

- Sale created.
- Sale voided.
- Purchase created.
- Payment recorded.
- Stock adjusted.
- Product deactivated.
- User created.
- Import executed.

Avoid logging sensitive authentication secrets.

---

# 35. DATA RETENTION / DELETION

MVP default:

### Master data

Prefer:

```text
Active / Inactive
```

rather than destructive deletion.

### Transactions

Prefer:

```text
Posted -> Void/Reverse
```

rather than delete.

The Phase 2 production policy should define retention requirements after customer/legal review.

---

# 36. BUSINESS SETTINGS

Initial tenant settings:

```text
Business Name
Business Address
Phone
Email
GST Number
Business Timezone
Currency
Invoice Prefix
Invoice Number Start
Low Stock Defaults
Allow Negative Stock [default false]
```

Tax configuration should remain simple until requirements are validated.

---

# 37. FEATURE FLAGS

Recommended lightweight feature configuration:

```text
TenantFeature
  TenantId
  FeatureKey
  Enabled
```

Potential future keys:

```text
advanced-pricing
multi-warehouse
returns
batch-expiry
mobile-orders
advanced-reports
```

Do not build a complex feature-management platform in MVP.

---

# 38. BACKGROUND JOBS

MVP should not depend heavily on background processing.

Potential future jobs:

- Large Excel import.
- Report generation.
- Notifications.
- Scheduled summaries.
- Data exports.

Design so a worker can be added later.

---

# 39. FILE STORAGE

For MVP:

- Store only necessary import/output files.
- Do not store arbitrary uploads.
- Validate file type and size.
- Avoid putting large files directly in database unless there is a specific need.

Future object storage can be introduced.

---

# 40. OBSERVABILITY

Minimum:

```text
Structured logs
Request ID / Trace ID
Health endpoint
Database health
Error monitoring
Latency/error metrics
Import failures
Critical transaction failures
```

Recommended endpoints:

```text
/health
/health/ready
/health/live
```

---

# 41. DEPLOYMENT ENVIRONMENTS

Minimum:

```text
Development
Staging
Production
```

Configuration must be environment-specific.

Never commit secrets.

Example:

```text
ConnectionStrings
JWT settings
Allowed origins
Storage settings
Logging
Feature flags
```

---

# 42. PRODUCTION TOPOLOGY

```text
                 Internet
                    |
             TLS Reverse Proxy
                    |
        +-----------+-----------+
        |                       |
 Angular static              API
                                |
                         ASP.NET Core
                                |
                           PostgreSQL
```

Optional later:

```text
Redis
Background Worker
Object Storage
Monitoring
Reporting DB
```

Only add when justified.

---

# 43. DOCKER

Recommended containers:

```text
reverse-proxy
api
postgres
```

For development, SQL Server can remain the local development database.

Production should use PostgreSQL.

The team must document how to run:

```text
docker compose up
```

for a local production-like environment.

---

# 44. BACKUP / DISASTER RECOVERY

Production must define:

- Backup frequency.
- Retention.
- Off-server backup.
- Encryption.
- Restore test schedule.
- Recovery Point Objective.
- Recovery Time Objective.

Minimum acceptable engineering requirement:

> A backup that has never been restored/tested is not considered a verified backup.

---

# 45. CI/CD

Recommended pipeline:

```text
Git push
  |
Build
  |
Unit tests
  |
Static analysis
  |
Integration tests
  |
Build Docker image
  |
Deploy staging
  |
Smoke tests
  |
Production approval
  |
Deploy
```

The exact CI provider can be chosen later.

---

# 46. QUALITY GATES

A feature is not complete when the screen works.

It is complete when:

- Domain rules implemented.
- API implemented.
- Validation implemented.
- Authorization implemented.
- Tenant isolation verified.
- DB migration verified.
- Unit/application tests added.
- API tests added.
- UI validation added.
- Error states handled.
- Logging added where necessary.
- Documentation updated.

---

# 47. TEST PLAN

## 47.1 Tenant tests

- User cannot access another tenant.
- API rejects invalid tenant context.
- Report cannot leak another tenant.
- Import cannot affect another tenant.

## 47.2 Sales tests

- Valid sale succeeds.
- Invalid product fails.
- Invalid customer fails.
- Insufficient stock fails.
- Stock rolls back on failure.
- Customer ledger rolls back on failure.
- Payment cannot exceed allowed amount unless explicitly permitted.
- Concurrent stock sale is safe.

## 47.3 Purchase tests

- Purchase increases stock.
- Invalid supplier fails.
- Invalid product fails.
- Failed purchase rolls back.

## 47.4 Payment tests

- Payment creates ledger credit.
- Outstanding changes correctly.
- Invalid customer fails.
- Negative payment fails.

## 47.5 Import tests

- Valid Excel succeeds.
- Missing required column fails.
- Invalid row is reported.
- Duplicate SKU is handled according to policy.
- Preview does not modify database.
- Execute modifies database only after confirmation.

---

# 48. SQL SERVER / POSTGRESQL TEST MATRIX

At minimum test:

| Area | SQL Server | PostgreSQL |
|---|---:|---:|
| Migrations | Yes | Yes |
| Product CRUD | Yes | Yes |
| Customer CRUD | Yes | Yes |
| Sales | Yes | Yes |
| Purchase | Yes | Yes |
| Inventory | Yes | Yes |
| Payments | Yes | Yes |
| Reports | Yes | Yes |
| Pagination | Yes | Yes |
| Date filters | Yes | Yes |
| Decimal/money | Yes | Yes |
| Concurrency | Yes | Yes |

Production acceptance must be based on PostgreSQL.

---

# 49. SEED / DEMO DATA

Seed only development/demo data.

Do not seed fake customers/products into production automatically.

Demo dataset:

```text
Tenant: Demo Wholesale

Products: 30
Customers: 15
Suppliers: 4
Purchases: 20+
Sales: 50+
Payments: 30+
```

The numbers are guidelines, not strict requirements.

---

# 50. PRODUCT DEMO SCRIPT

A five-minute demo should follow:

```text
1. Login
2. Dashboard
3. Show products
4. Show retail shops
5. Create purchase
6. Show stock increase
7. Create sale
8. Show stock decrease
9. Record payment
10. Show outstanding
11. Open customer statement
12. Open business report
13. Show Excel import
```

This sequence demonstrates the complete value proposition.

---

# 51. MVP IMPLEMENTATION MILESTONES

## M0 — Foundation

Deliver:

- Repository.
- Solution.
- Angular shell.
- .NET API shell.
- Flutter shell.
- Coding standards.
- CI skeleton.
- Local environment.
- Docker development setup.

## M1 — Tenant + Identity

Deliver:

- Tenant.
- Users.
- Authentication.
- Roles.
- Tenant context.
- Authorization.
- Security tests.

## M2 — Masters

Deliver:

- Products.
- Customers.
- Suppliers.
- Search/filter/pagination.
- Basic import framework.

## M3 — Inventory

Deliver:

- Opening stock.
- Stock balance.
- Inventory transactions.
- Stock adjustment.
- Stock reports.

## M4 — Purchase

Deliver:

- Purchase entry.
- Purchase list/detail.
- Stock increase.
- Purchase reports.

## M5 — Sales

Deliver:

- Sales entry.
- Sales list/detail.
- Stock decrease.
- Customer ledger.
- Invoice.

## M6 — Payments

Deliver:

- Payment entry.
- Outstanding.
- Customer statement.
- Payment reports.

## M7 — Dashboard + Reports

Deliver:

- Dashboard.
- Core business reports.
- Export.

## M8 — Excel Import

Deliver:

- Product import.
- Customer import.
- Supplier import.
- Opening stock import.
- Preview/errors.

## M9 — Flutter MVP

Deliver:

- Login.
- Dashboard.
- Customers.
- Products.
- Sales.
- Payments.
- Outstanding.
- Selected reports.

## M10 — Production Hardening

Deliver:

- Security review.
- PostgreSQL integration.
- SQL Server compatibility.
- Backup.
- Monitoring.
- CI/CD.
- Production deployment.
- Demo environment.

---

# 52. DEFINITION OF DONE FOR MVP

All of the following must be true:

```text
[ ] Tenant isolation verified
[ ] Authentication works
[ ] Role authorization works
[ ] Product CRUD works
[ ] Customer CRUD works
[ ] Supplier CRUD works
[ ] Opening stock works
[ ] Purchase works
[ ] Sale works
[ ] Stock ledger works
[ ] Customer ledger works
[ ] Payment works
[ ] Outstanding works
[ ] Dashboard works
[ ] Core reports work
[ ] Excel preview works
[ ] Excel execute works
[ ] Invoice works
[ ] Flutter core workflows work
[ ] PostgreSQL production works
[ ] SQL Server development works
[ ] Integration tests pass
[ ] Backup/restore verified
[ ] Health checks work
[ ] Logging works
[ ] Demo data exists
[ ] Demo script works
```

---

# 53. RISKS

## Risk 1 — Overbuilding

Mitigation:

- Keep MVP scope narrow.
- Validate with real wholesalers.

## Risk 2 — Tenant data leak

Mitigation:

- Central tenant context.
- Tenant-scoped repositories/query services.
- Automated cross-tenant tests.

## Risk 3 — Stock inconsistency

Mitigation:

- Inventory-owned stock operations.
- Atomic transactions.
- Concurrency handling.
- Movement ledger.

## Risk 4 — SQL Server/PostgreSQL incompatibility

Mitigation:

- Provider-neutral LINQ.
- Integration testing against both.
- Provider-specific code isolation.

## Risk 5 — Reporting becomes slow

Mitigation:

- Projections.
- Correct indexes.
- Pagination.
- Later reporting database if necessary.

## Risk 6 — Complex UX

Mitigation:

- Use business language.
- Prototype with actual wholesalers.
- Remove unnecessary screens/settings.

## Risk 7 — Microservice complexity too early

Mitigation:

- Modular monolith.
- Explicit contracts.
- Extract only after evidence.

---

# 54. OPEN PRODUCT DECISIONS

These must be confirmed before production, but reasonable MVP defaults may be used for prototype:

1. Exact GST/tax behavior.
2. Stock valuation method.
3. Negative stock policy — default false.
4. Quantity decimal precision.
5. Invoice numbering/reset policy.
6. Customer matching rules during import.
7. Supplier payable tracking depth.
8. Returns behavior.
9. Whether sale is called Invoice/Order in the customer's terminology.
10. Whether discounts are percentage, amount, or both.
11. Tax calculation model.
12. Whether prices are tax-inclusive/exclusive.
13. Exact Flutter offline requirements.
14. Exact backup RPO/RTO.
15. Subscription/billing model for SaaS.
16. Future warehouse model.

Do not allow unresolved financial rules to become hidden assumptions in production.

---

# 55. FUTURE MICROSERVICE EXTRACTION MAP

Potential candidates:

```text
Inventory
Sales
Reporting
Imports
Notifications
Identity
```

Do not extract simply because they are candidates.

Extraction should happen only after measurable justification.

The modular monolith must already have:

```text
Module-owned domain logic
Module-owned application logic
Module contracts
Controlled persistence access
No direct cross-module repository access
Clear integration points
```

---

# 56. COMMERCIAL SaaS READINESS

Even if subscription billing is not MVP, the platform should conceptually distinguish:

```text
Platform
  |
Tenants
  |
Tenant users
  |
Tenant features
  |
Tenant data
```

Future platform capabilities:

- Subscription plan.
- Feature entitlement.
- Trial period.
- Usage limits.
- Tenant suspension.
- Billing integration.
- Support/admin console.

Do not build these before the core business workflow is validated.

---

# 57. CODING STANDARDS

The implementation AI should:

- Prefer readable code.
- Keep methods focused.
- Avoid unnecessary abstractions.
- Use dependency injection.
- Use async APIs.
- Use cancellation tokens for long-running operations where appropriate.
- Validate inputs.
- Avoid magic strings.
- Use enums/value objects where domain meaning benefits.
- Keep DTOs separate from persistence models.
- Use explicit naming.
- Keep module dependencies one-directional.
- Add tests alongside features.
- Document non-obvious business rules.

---

# 58. DOCUMENTATION REQUIRED IN REPOSITORY

The implementation repository should contain:

```text
README.md
docs/
  architecture/
  api/
  database/
  modules/
  deployment/
  testing/
  decisions/
  imports/
```

At minimum:

- Architecture overview.
- Local setup.
- Database setup.
- Migration instructions.
- API documentation.
- Module rules.
- Deployment guide.
- Backup/restore guide.
- Demo guide.
- Known decisions.

---

# 59. ARCHITECTURE DECISION RECORDS

For significant choices, create ADRs:

```text
ADR-001 Modular Monolith
ADR-002 SQL Server + PostgreSQL EF Core portability
ADR-003 One DbContext initially
ADR-004 Repository strategy
ADR-005 Multi-tenancy strategy
ADR-006 Reporting architecture
ADR-007 Flutter mobile
ADR-008 Production deployment
```

Future decisions should be added instead of silently changing architecture.

---

# 60. FINAL IMPLEMENTATION PRINCIPLE

The product must optimize for:

```text
Simplicity
    +
Correctness
    +
Maintainability
    +
Low operating cost
    +
Future extensibility
```

Not:

```text
Maximum number of modules
Maximum architecture complexity
Maximum cloud services
```

The product succeeds if a real wholesaler can replace meaningful parts of their Excel/manual process with less effort and obtain better business visibility.

---

# 61. FINAL AI IMPLEMENTATION COMMAND

Use this instruction after reviewing the specification:

> Implement the Wholesale/Distributor Business Management SaaS exactly according to the Phase 1 and Phase 2 documents.
>
> Start with the foundation and implement vertically by milestone.
>
> Do not introduce full microservices.
>
> Use ASP.NET Core/.NET, Angular, Flutter and EF Core.
>
> Keep the backend database-provider neutral so SQL Server works in development and PostgreSQL works in production.
>
> Use a modular monolith with strict module boundaries.
>
> Use one DbContext initially.
>
> Use selective repositories/query services; do not create a generic repository over every table.
>
> Keep tenant isolation as a critical security boundary.
>
> Do not expose EF entities through APIs.
>
> Keep authoritative business calculations on the server.
>
> Make sales, purchases, inventory and payments transactionally correct.
>
> Build automated tests with every milestone.
>
> Do not implement future ERP features unless explicitly approved.
>
> When a requirement is ambiguous, consult the Open Product Decisions section and use the simplest documented MVP default.
>
> At the end of every milestone, provide:
>
> 1. What was implemented.
> 2. Files/projects changed.
> 3. Database changes.
> 4. API changes.
> 5. Tests added.
> 6. Known limitations.
> 7. Next milestone.
>
> Do not proceed by hiding architectural assumptions.

---

# 62. PHASE 2 EXIT CRITERIA

Phase 2 is complete when:

- Every MVP module has functional requirements.
- Every primary workflow has acceptance criteria.
- Major screens are defined.
- API contracts are defined.
- Domain entities are defined.
- Database schema is defined.
- Tenant strategy is defined.
- EF Core/provider strategy is defined.
- Transaction boundaries are defined.
- Security model is defined.
- Reporting catalog is defined.
- Excel import rules are defined.
- Testing strategy is defined.
- Deployment strategy is defined.
- Open decisions are explicitly listed.
- Implementation milestones are actionable.

At this point, the AI should be able to begin implementation without inventing the application's architecture.

---

# 63. PRODUCT NORTH STAR

> **Make wholesale distribution operations easier, faster and more visible without making the business owner learn complicated software.**

The application should start simple, prove value quickly, and grow into a larger platform only when real customer usage demonstrates the need.
