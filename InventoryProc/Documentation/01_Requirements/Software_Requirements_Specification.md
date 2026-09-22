# Software Requirements Specification (SRS)
## Wholesale/Distributor Business Management SaaS

**Document Version:** 1.0
**Date:** 2026-08-29
**Status:** Living Document
**Project:** InventoryProc - Wholesale Distributor SaaS MVP

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-08-29 | Development Team | Initial SRS created from Phase 1 & Phase 2 specifications |

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Product Vision & Scope](#2-product-vision--scope)
3. [Target Users](#3-target-users)
4. [Functional Requirements](#4-functional-requirements)
5. [Non-Functional Requirements](#5-non-functional-requirements)
6. [Technology Stack](#6-technology-stack)
7. [Business Rules](#7-business-rules)
8. [Constraints & Assumptions](#8-constraints--assumptions)
9. [Success Criteria](#9-success-criteria)

---

## 1. Introduction

### 1.1 Purpose
This document specifies the requirements for a multi-tenant SaaS application designed for wholesalers and distributors who purchase goods from suppliers and sell them to retail shops. The system aims to replace manual Excel-based processes with an integrated, simple-to-use business management platform.

### 1.2 Scope
This is an MVP (Minimum Viable Product) release focused on core business operations:
- Product, Customer, and Supplier master data management
- Purchase/Stock-in operations
- Sales/Billing operations
- Inventory management
- Payment tracking and customer outstanding management
- Business reporting and analytics
- Excel import/export capabilities

**OUT OF SCOPE (Post-MVP):**
- Full ERP/accounting/general ledger
- Manufacturing operations
- Complex approval workflows
- Batch/expiry tracking
- Multi-warehouse management
- Route optimization
- Multi-currency support

### 1.3 Document Conventions
- **LOCKED:** Technology or architectural decision that must not be changed
- **REQUIRED:** Mandatory for MVP
- **OPTIONAL:** Can be deferred to post-MVP
- **FUTURE:** Planned for future releases

---

## 2. Product Vision & Scope

### 2.1 Vision Statement
Build a practical, affordable SaaS product that helps wholesalers/distributors transition from manual/Excel-based processes to a modern, integrated business management system that reduces operational costs, minimizes errors, and provides real-time business visibility.

### 2.2 Core Business Loop
```
Business (Tenant)
    │
    ├── Products (Master Data)
    ├── Customers / Retail Shops (Master Data)
    ├── Suppliers (Master Data)
    │
    ├── Purchase / Stock In ────► Inventory (Increases Stock)
    │
    ├── Sales / Billing ────────► Inventory (Decreases Stock)
    │                           └─► Customer Ledger (Outstanding)
    │
    ├── Payments ───────────────► Customer Outstanding (Reduces)
    │
    └── Reports ────────────────► Business Visibility & Analytics
```

### 2.3 Key Objectives
1. **Reduce Manual Work:** Automate repetitive data entry and calculations
2. **Improve Stock Visibility:** Real-time inventory tracking across all operations
3. **Track Customer Outstanding:** Automated customer ledger and payment tracking
4. **Enable Data Migration:** Excel import for seamless transition from spreadsheets
5. **Provide Business Insights:** Comprehensive reporting on sales, purchases, inventory, and financials
6. **Simplify Operations:** Intuitive UX designed for non-technical users

---

## 3. Target Users

### 3.1 Primary Customer Profile
- **Business Type:** Wholesalers/Distributors
- **Business Model:** Buy from suppliers → Store inventory → Sell to retail shops
- **Current System:** Excel, notebooks, or basic software
- **Pain Points:** Manual data entry, stock discrepancies, payment tracking, lack of visibility
- **Technical Proficiency:** Low to moderate; needs simple, intuitive interface

### 3.2 User Roles

#### 3.2.1 Admin / Business Owner
**Access Level:** Full
**Responsibilities:**
- Complete business configuration
- User management
- Master data management
- All operational functions
- Financial and strategic reporting

**Key Features:**
- Tenant settings and configuration
- User role assignment
- Access to all modules
- Complete reporting suite

#### 3.2.2 Manager
**Access Level:** Operational
**Responsibilities:**
- Day-to-day operations oversight
- Inventory management
- Purchase and sales approval (if configured)
- Operational reporting

**Key Features:**
- Product, customer, supplier management
- Purchase and sales operations
- Inventory monitoring
- Operational reports

#### 3.2.3 Sales Staff
**Access Level:** Sales-focused
**Responsibilities:**
- Customer lookup and interaction
- Sales/billing creation
- Payment recording
- Customer outstanding inquiry

**Key Features:**
- Customer search and details
- Create sales invoices
- Record payments
- View customer outstanding

#### 3.2.4 Viewer
**Access Level:** Read-only
**Responsibilities:**
- Monitor business performance
- View reports and dashboards

**Key Features:**
- Dashboard access
- Report viewing (no editing)
- Data inquiry (no modifications)

---

## 4. Functional Requirements

### 4.1 Multi-Tenancy & Platform Management

#### FR-1.1: Tenant Management (REQUIRED)
- **Description:** Each customer business operates as an isolated tenant
- **Requirements:**
  - Unique tenant identification (ID, Code, Name)
  - Tenant business information (Name, Email, Phone, Address, GST Number)
  - Tenant activation/deactivation
  - Complete data isolation between tenants
  - Tenant context from authentication token

**Acceptance Criteria:**
- ✓ Admin can create new tenant with complete business details
- ✓ All tenant data is completely isolated (no cross-tenant access)
- ✓ Tenant context automatically applied to all queries and operations
- ✓ Tenant can be activated/deactivated
- ✓ Deactivated tenants cannot access the system

#### FR-1.2: Identity & Access Management (REQUIRED)
- **Description:** Secure user authentication and role-based authorization
- **Requirements:**
  - User registration and login
  - Password hashing (strong algorithm)
  - Role-based access control (Admin, Manager, Sales Staff, Viewer)
  - Tenant-specific user management
  - Session management with JWT tokens

**Acceptance Criteria:**
- ✓ Users can register and log in securely
- ✓ Passwords are hashed using industry-standard algorithms
- ✓ Users assigned to specific tenant and role
- ✓ Access control enforced based on roles
- ✓ Session tokens include tenant context

---

### 4.2 Master Data Management

#### FR-2.1: Product Management (REQUIRED)
- **Description:** Comprehensive product catalog management
- **Requirements:**
  - Product creation, update, read, delete (soft delete)
  - Product attributes:
    - Name, SKU, Barcode
    - Category, Brand, Unit of Measurement
    - Purchase Price, Selling Price
    - Minimum Stock Level
    - Active/Inactive status
  - Product search and filtering
  - Product categorization

**Acceptance Criteria:**
- ✓ Users can create products with all required attributes
- ✓ Products can be searched by name, SKU, barcode
- ✓ Products can be filtered by category, brand, active status
- ✓ Products can be marked inactive (not deleted)
- ✓ Product listing supports pagination

#### FR-2.2: Customer Management (REQUIRED)
- **Description:** Retail shop/customer master data
- **Requirements:**
  - Customer creation, update, read, delete (soft delete)
  - Customer attributes:
    - Shop Name, Owner Name
    - Mobile, Email
    - Address (City, State, Pincode)
    - GST Number
    - Credit Limit, Credit Days
    - Opening Balance
    - Active/Inactive status
  - Customer search and filtering
  - Customer ledger view

**Acceptance Criteria:**
- ✓ Users can create customers with complete business details
- ✓ Customers can be searched by name, mobile, GST number
- ✓ Credit limits and terms can be configured
- ✓ Opening balance can be set during initial setup
- ✓ Customer ledger shows all transactions

#### FR-2.3: Supplier Management (REQUIRED)
- **Description:** Supplier master data management
- **Requirements:**
  - Supplier creation, update, read, delete (soft delete)
  - Supplier attributes:
    - Name, Contact Person
    - Mobile, Email
    - Address (City, State, Pincode)
    - GST Number
    - Opening Balance
    - Active/Inactive status
  - Supplier search and filtering

**Acceptance Criteria:**
- ✓ Users can create suppliers with complete details
- ✓ Suppliers can be searched by name, contact, GST number
- ✓ Opening balance can be set
- ✓ Supplier listing supports pagination and filtering

---

### 4.3 Inventory Management

#### FR-3.1: Stock Balance Tracking (REQUIRED)
- **Description:** Real-time inventory balance for all products
- **Requirements:**
  - Current stock balance per product
  - Quantity on hand tracking
  - Reserved quantity tracking (future)
  - Low stock alerts based on minimum stock level
  - Stock valuation (quantity × cost)

**Acceptance Criteria:**
- ✓ Stock balance updated automatically on purchase/sale
- ✓ Real-time stock visibility for all products
- ✓ Low stock alerts generated when stock < minimum level
- ✓ Stock balance is tenant-specific
- ✓ Stock cannot go negative (validation on sales)

#### FR-3.2: Inventory Transactions (REQUIRED)
- **Description:** Complete audit trail of all inventory movements
- **Requirements:**
  - Transaction types:
    - Opening Stock
    - Purchase (Stock In)
    - Sale (Stock Out)
    - Adjustment
    - Return (future)
    - Transfer In/Out (future)
  - Transaction attributes:
    - Product, Quantity
    - Transaction Type, Date
    - Reference Type and ID (Purchase ID, Sale ID, etc.)
    - Created By, Created At
  - Complete transaction history per product
  - Stock movement reports

**Acceptance Criteria:**
- ✓ Every stock change creates an inventory transaction
- ✓ Transactions are immutable (cannot be edited/deleted)
- ✓ Transaction history shows complete audit trail
- ✓ Transactions linked to source documents (Purchase, Sale)
- ✓ Stock movement reports available by date range

#### FR-3.3: Opening Stock Entry (REQUIRED)
- **Description:** Initial stock loading for new tenants
- **Requirements:**
  - Bulk opening stock entry
  - Excel import for opening stock
  - Opening stock adjustment
  - Opening stock valuation

**Acceptance Criteria:**
- ✓ Users can enter opening stock for products
- ✓ Opening stock can be imported via Excel
- ✓ Opening stock creates inventory transactions
- ✓ Opening stock includes quantity and cost

---

### 4.4 Purchase Management

#### FR-4.1: Purchase Order/Invoice Entry (REQUIRED)
- **Description:** Record purchases from suppliers
- **Requirements:**
  - Purchase header:
    - Supplier selection
    - Invoice Number, Purchase Date
    - Sub-Total, Discount, Tax, Grand Total
    - Notes
  - Purchase items (line items):
    - Product selection
    - Quantity, Unit Price
    - Discount, Tax
    - Line Total (auto-calculated)
  - Purchase save and confirmation
  - Purchase search and listing

**Acceptance Criteria:**
- ✓ Users can create purchase with multiple line items
- ✓ Totals calculated automatically on server-side
- ✓ Purchase updates inventory (increases stock)
- ✓ Purchase creates inventory transactions
- ✓ Purchase cannot be created without supplier and products
- ✓ Purchase history available with filters

#### FR-4.2: Purchase-Inventory Integration (REQUIRED)
- **Description:** Automatic inventory update on purchase
- **Requirements:**
  - Stock increased when purchase is confirmed
  - Inventory transactions created automatically
  - Atomic transaction (purchase + stock update together)
  - Rollback on failure

**Acceptance Criteria:**
- ✓ Purchase confirmation increases stock automatically
- ✓ Each purchase item creates inventory transaction
- ✓ If inventory update fails, purchase is rolled back
- ✓ Stock balance reflects purchase immediately

---

### 4.5 Sales Management

#### FR-5.1: Sales Invoice Entry (REQUIRED)
- **Description:** Create sales invoices for customers
- **Requirements:**
  - Sales header:
    - Customer selection
    - Invoice Number (auto-generated or manual)
    - Sale Date
    - Sub-Total, Discount, Tax, Grand Total
    - Paid Amount, Due Amount
    - Payment Status (Paid, Partial, Unpaid)
    - Notes
  - Sales items (line items):
    - Product selection with stock validation
    - Quantity, Unit Price
    - Discount, Tax
    - Line Total (auto-calculated)
  - Payment recording at time of sale (optional)
  - Sales search and listing

**Acceptance Criteria:**
- ✓ Users can create sales invoice with multiple line items
- ✓ Stock availability validated before sale confirmation
- ✓ Totals calculated automatically on server-side
- ✓ Sale updates inventory (decreases stock)
- ✓ Sale creates customer ledger entry
- ✓ Payment can be recorded partially or fully
- ✓ Invoice printable/downloadable

#### FR-5.2: Sales-Inventory Integration (REQUIRED)
- **Description:** Automatic inventory and ledger update on sale
- **Requirements:**
  - Stock decreased when sale is confirmed
  - Inventory transactions created automatically
  - Customer ledger updated (debit for sale amount)
  - Payment ledger updated (credit for paid amount)
  - Atomic transaction (sale + stock + ledger together)
  - Rollback on failure

**Acceptance Criteria:**
- ✓ Sale confirmation decreases stock automatically
- ✓ Each sale item creates inventory transaction
- ✓ Customer ledger shows sale as debit
- ✓ If paid amount > 0, payment ledger entry created
- ✓ If any update fails, entire sale is rolled back
- ✓ Stock cannot go negative (validation enforced)

#### FR-5.3: Invoice Generation (REQUIRED)
- **Description:** Printable/downloadable sales invoice
- **Requirements:**
  - Professional invoice layout
  - Tenant business information (header)
  - Customer information
  - Product line items with prices
  - Tax and discount breakdown
  - Grand total and payment status
  - PDF export

**Acceptance Criteria:**
- ✓ Invoice displays all sale details correctly
- ✓ Invoice includes tenant logo and business details
- ✓ Invoice can be printed directly
- ✓ Invoice can be downloaded as PDF

---

### 4.6 Payment Management

#### FR-6.1: Payment Recording (REQUIRED)
- **Description:** Record customer payments against outstanding balance
- **Requirements:**
  - Payment attributes:
    - Customer selection
    - Payment Date, Amount
    - Payment Method (Cash, Cheque, Bank Transfer, UPI, etc.)
    - Reference Number (for bank/UPI transactions)
    - Notes
  - Payment creates customer ledger entry (credit)
  - Payment history per customer

**Acceptance Criteria:**
- ✓ Users can record payment for any customer
- ✓ Payment reduces customer outstanding balance
- ✓ Payment creates customer ledger entry (credit)
- ✓ Payment method and reference captured
- ✓ Payment history available by customer and date

#### FR-6.2: Customer Ledger (REQUIRED)
- **Description:** Complete transaction history for each customer
- **Requirements:**
  - Ledger entry types:
    - Opening Balance (Debit/Credit)
    - Sale (Debit)
    - Payment (Credit)
    - Adjustment (future)
  - Ledger attributes:
    - Entry Date, Entry Type
    - Reference Type and ID
    - Debit Amount, Credit Amount
    - Running Balance
    - Notes
  - Customer statement generation

**Acceptance Criteria:**
- ✓ Ledger shows complete transaction history
- ✓ Running balance calculated correctly
- ✓ Ledger entries immutable
- ✓ Customer statement downloadable
- ✓ Statement shows opening balance, transactions, closing balance

---

### 4.7 Reporting & Analytics

#### FR-7.1: Dashboard (REQUIRED)
- **Description:** Business overview and key metrics
- **Requirements:**
  - Key metrics:
    - Today's Sales, This Month Sales
    - Total Outstanding (Receivables)
    - Low Stock Products Count
    - Recent Sales (last 10)
    - Top Customers (by sales)
    - Top Products (by quantity sold)
  - Date range filters
  - Quick action buttons

**Acceptance Criteria:**
- ✓ Dashboard shows accurate, real-time data
- ✓ Calculations performed on server-side
- ✓ Dashboard loads within 2 seconds
- ✓ Responsive design for mobile/tablet

#### FR-7.2: Sales Reports (REQUIRED)
- **Description:** Comprehensive sales analytics
- **Requirements:**
  - Sales Summary Report:
    - Total Sales, Total Items Sold
    - Average Order Value
    - Date range filter
  - Sales by Product Report:
    - Product-wise sales breakdown
    - Quantity sold, Total amount
    - Sorting and filtering
  - Sales by Customer Report:
    - Customer-wise sales breakdown
    - Total amount, Number of invoices
    - Sorting and filtering
  - Sales Detail Report:
    - Complete invoice-level details
    - Filterable by date, customer, product

**Acceptance Criteria:**
- ✓ Reports show accurate data for selected date range
- ✓ Reports exportable to Excel/PDF
- ✓ Reports support pagination for large datasets
- ✓ Calculations performed on server-side

#### FR-7.3: Purchase Reports (REQUIRED)
- **Description:** Purchase analytics and supplier analysis
- **Requirements:**
  - Purchase Summary Report:
    - Total Purchases, Total Items Purchased
    - Average Purchase Value
    - Date range filter
  - Purchase by Supplier Report:
    - Supplier-wise purchase breakdown
    - Total amount, Number of purchases
    - Sorting and filtering

**Acceptance Criteria:**
- ✓ Reports show accurate purchase data
- ✓ Reports exportable to Excel/PDF
- ✓ Reports support pagination

#### FR-7.4: Inventory Reports (REQUIRED)
- **Description:** Stock analysis and movement tracking
- **Requirements:**
  - Current Stock Report:
    - All products with current stock levels
    - Stock value (quantity × cost)
    - Filter by category, brand
  - Low Stock Report:
    - Products below minimum stock level
    - Sortable by stock level
  - Stock Movement Report:
    - Product-wise stock in/out
    - Date range filter
    - Transaction type filter
  - Slow Moving Products Report:
    - Products with low sales velocity
    - Configurable period

**Acceptance Criteria:**
- ✓ Stock reports show real-time accurate data
- ✓ Low stock alerts updated automatically
- ✓ Movement report shows complete audit trail
- ✓ Reports exportable

#### FR-7.5: Customer Outstanding Reports (REQUIRED)
- **Description:** Receivables tracking and aging
- **Requirements:**
  - Customer Outstanding Summary:
    - All customers with outstanding balances
    - Total outstanding amount
    - Sortable by amount, days overdue
  - Customer Statement:
    - Complete ledger for selected customer
    - Opening balance, transactions, closing balance
    - Date range filter
  - Aging Report (future):
    - Outstanding breakdown by age (0-30, 31-60, 61-90, 90+)

**Acceptance Criteria:**
- ✓ Outstanding reports show accurate real-time balances
- ✓ Customer statement printable/downloadable
- ✓ Reports include payment history

---

### 4.8 Excel Import/Export

#### FR-8.1: Master Data Import (REQUIRED)
- **Description:** Bulk import of master data from Excel
- **Requirements:**
  - Import entities:
    - Products
    - Customers
    - Suppliers
    - Opening Stock
  - Import workflow:
    1. Upload Excel file
    2. Validate file format and columns
    3. Validate row data (business rules)
    4. Preview import with errors highlighted
    5. Confirm and execute import
    6. Show results (imported, skipped, errors)
  - Import template download
  - Error reporting with line numbers

**Acceptance Criteria:**
- ✓ Users can download Excel templates for each entity
- ✓ File upload validates format before processing
- ✓ Data validation follows same rules as manual entry
- ✓ Preview shows exactly what will be imported
- ✓ Invalid rows clearly marked with error messages
- ✓ Users can fix errors and re-upload
- ✓ Import is atomic (all or nothing) per entity
- ✓ Import results show summary and detailed errors

#### FR-8.2: Report Export (REQUIRED)
- **Description:** Export reports to Excel/PDF
- **Requirements:**
  - All reports exportable
  - Excel format for data analysis
  - PDF format for printing/archiving
  - Export preserves filters and date ranges

**Acceptance Criteria:**
- ✓ Export button available on all reports
- ✓ Excel export includes all columns and data
- ✓ PDF export includes formatting and headers
- ✓ Export reflects current filters and selections

---

## 5. Non-Functional Requirements

### 5.1 Performance

#### NFR-1.1: Response Time
- Dashboard: < 2 seconds
- Master data listing: < 1 second
- Sales/Purchase save: < 2 seconds
- Reports (standard): < 3 seconds
- Search/lookup: < 1 second

#### NFR-1.2: Concurrent Users
- Support minimum 50 concurrent users per tenant
- System should scale to 1000+ total users across all tenants

#### NFR-1.3: Data Volume
- Support minimum 100,000 products per tenant
- Support minimum 10,000 customers per tenant
- Support minimum 1,000,000 transactions per tenant per year

### 5.2 Security

#### NFR-2.1: Tenant Isolation (CRITICAL)
- **MANDATORY:** Complete data isolation between tenants
- No cross-tenant data access under any circumstance
- Tenant context validated on every request
- Tenant ID never trusted from client

#### NFR-2.2: Authentication & Authorization
- Strong password hashing (bcrypt or equivalent)
- JWT-based session management
- HTTPS only (no HTTP in production)
- Role-based access control strictly enforced

#### NFR-2.3: Data Protection
- Sensitive data encrypted at rest
- No passwords or secrets in logs
- SQL injection prevention
- XSS protection
- CSRF protection
- File upload validation
- Rate limiting for APIs

### 5.3 Reliability

#### NFR-3.1: Availability
- Target: 99.5% uptime (excluding planned maintenance)
- Maximum planned downtime: 4 hours/month
- Automatic health checks and alerting

#### NFR-3.2: Data Integrity
- ACID transactions for critical operations (sale, purchase, payment)
- Database constraints and foreign keys enforced
- No data loss on system failure
- Automated daily backups
- Backup retention: 30 days minimum

#### NFR-3.3: Error Handling
- Graceful error handling (no unhandled exceptions)
- User-friendly error messages
- Detailed error logging for debugging
- Automatic rollback on transaction failure

### 5.4 Usability

#### NFR-4.1: User Interface
- Responsive design (desktop, tablet, mobile)
- Keyboard-friendly data entry
- Consistent UI/UX across modules
- Maximum 3 clicks for common operations
- Intuitive navigation

#### NFR-4.2: Learning Curve
- Non-technical users can start using within 1 hour
- Context-sensitive help available
- Tooltips for complex features
- Demo data included for training

### 5.5 Maintainability

#### NFR-5.1: Code Quality
- Clean, modular code architecture
- Strong module boundaries
- Unit test coverage > 70%
- Integration test coverage for critical paths
- API documentation (Swagger/OpenAPI)

#### NFR-5.2: Deployment
- Docker-based deployment
- CI/CD pipeline automated
- Blue-green deployment capability
- Rollback procedure documented and tested

### 5.6 Compatibility

#### NFR-6.1: Browser Support
- Modern browsers (Chrome, Firefox, Edge, Safari)
- Last 2 versions of each browser
- No IE support required

#### NFR-6.2: Database Portability
- **CRITICAL:** Support both SQL Server (dev) and PostgreSQL (prod)
- No database-specific SQL in business logic
- EF Core migrations for both providers
- Integration tests run on both databases

---

## 6. Technology Stack

### 6.1 LOCKED Technology Decisions

| Component | Technology | Justification |
|-----------|-----------|---------------|
| **Web Frontend** | Angular + TypeScript | Modern framework, strong typing, enterprise-ready |
| **Mobile** | Flutter | Cross-platform, native performance, shared codebase |
| **Backend** | ASP.NET Core / .NET | Mature, performant, excellent business app support |
| **ORM** | Entity Framework Core | First-class .NET support, migrations, LINQ |
| **Dev Database** | SQL Server | Local development compatibility |
| **Prod Database** | PostgreSQL | Cost-effective, reliable, open-source |
| **Architecture** | Modular Monolith | Simple deployment, strong boundaries, extractable modules |
| **API Style** | REST/JSON | Standard, simple, well-understood |

### 6.2 Supporting Technologies
- **Authentication:** JWT tokens, ASP.NET Core Identity
- **API Documentation:** Swagger/OpenAPI
- **Logging:** Serilog or NLog
- **Testing:** xUnit, Moq, FluentAssertions
- **CI/CD:** GitHub Actions or Azure DevOps
- **Containerization:** Docker
- **Reverse Proxy:** Nginx or Traefik

---

## 7. Business Rules

### 7.1 Inventory Rules
- Stock cannot go negative (sale validation required)
- Every stock change must create inventory transaction
- Inventory transactions are immutable (no edit/delete)
- Opening stock creates inventory transaction

### 7.2 Sales Rules
- Sale requires valid customer
- Sale item quantity must not exceed available stock
- Sale grand total calculated on server (client not trusted)
- Paid amount cannot exceed grand total
- Due amount = Grand Total - Paid Amount
- Payment status:
  - Unpaid: Paid Amount = 0
  - Partial: 0 < Paid Amount < Grand Total
  - Paid: Paid Amount = Grand Total

### 7.3 Payment Rules
- Payment requires valid customer
- Payment amount must be > 0
- Payment creates customer ledger credit entry
- Multiple payments allowed per sale

### 7.4 Customer Ledger Rules
- Opening balance creates initial ledger entry
- Sale creates debit entry
- Payment creates credit entry
- Ledger entries are immutable
- Running balance = Opening + Sum(Debits) - Sum(Credits)

### 7.5 Multi-Tenancy Rules
- All entities (except global config) include TenantId
- Tenant context from authenticated user token
- All queries automatically filtered by TenantId
- No cross-tenant access allowed
- Unique constraints must include TenantId where applicable

---

## 8. Constraints & Assumptions

### 8.1 Constraints
- MVP must be delivered as modular monolith (no microservices)
- Must support both SQL Server (dev) and PostgreSQL (prod)
- No generic repository pattern (selective repositories only)
- Must use Angular for web (no React/Vue)
- Must use Flutter for mobile (no React Native/Ionic)
- Must use .NET backend (no Node.js)

### 8.2 Assumptions
- Users have stable internet connection
- Users understand basic business concepts (sales, purchases, inventory)
- Single warehouse per tenant (multi-warehouse in future)
- Single currency per tenant (INR assumed, configurable later)
- Tax calculation is simple (single GST rate per item, complex tax later)
- No offline mode in MVP
- English language only in MVP

### 8.3 Dependencies
- Hosting infrastructure (cloud or on-premise)
- Database server (SQL Server for dev, PostgreSQL for prod)
- Email service for notifications (future)
- SMS service for alerts (future)

---

## 9. Success Criteria

### 9.1 MVP Definition of Done

The MVP is considered complete when:

✓ **Authentication & Authorization**
- User can register and login securely
- Role-based access control working
- Tenant isolation verified

✓ **Master Data Management**
- Products, Customers, Suppliers can be created/edited
- Master data can be imported from Excel
- Search and filtering works correctly

✓ **Inventory Management**
- Stock balance tracked accurately
- Opening stock can be loaded
- Inventory transactions recorded for all movements

✓ **Purchase Operations**
- Purchases can be recorded with line items
- Purchase updates inventory automatically
- Purchase history available

✓ **Sales Operations**
- Sales invoices can be created
- Stock validation prevents overselling
- Sales update inventory and customer ledger
- Invoices printable/downloadable

✓ **Payment Management**
- Payments can be recorded
- Customer outstanding calculated correctly
- Customer statements generated

✓ **Reporting**
- Dashboard shows real-time business metrics
- All required reports functional and accurate
- Reports exportable to Excel/PDF

✓ **Quality & Security**
- All critical features have automated tests
- Tenant isolation verified through testing
- Security vulnerabilities addressed
- Both SQL Server and PostgreSQL tested

✓ **Deployment**
- Application deployable via Docker
- Database migrations work on both providers
- Backup and restore procedures tested
- Demo data available

### 9.2 Business Success Metrics (Post-Launch)
- 10+ active tenant businesses within 3 months
- 90% user satisfaction rating
- < 5% critical bug rate
- Average response time < 2 seconds
- 99%+ uptime after first month

---

## Appendix A: Glossary

| Term | Definition |
|------|------------|
| **Tenant** | A customer business entity in the multi-tenant system |
| **Master Data** | Core reference data (Products, Customers, Suppliers) |
| **Stock Balance** | Current quantity of a product in inventory |
| **Inventory Transaction** | Record of stock movement (in/out) |
| **Customer Ledger** | Complete transaction history for a customer |
| **Outstanding** | Amount owed by customer (unpaid sales) |
| **SKU** | Stock Keeping Unit - unique product identifier |
| **GST** | Goods and Services Tax |
| **Modular Monolith** | Single deployment with strong logical module boundaries |

---

## Appendix B: References

- Phase 1 Master Document: `../Intial_Doc_prompts/Wholesale_Distributor_SaaS_Phase_1_Master_Document.md`
- Phase 2 Full Specification: `../Intial_Doc_prompts/Wholesale_Distributor_SaaS_Phase_2_Full_Application_Specification.md`
- Architecture Document: `../02_Architecture/Architecture_Design.md`
- Database Schema: `../03_Database/Database_Schema.md`
- API Specification: `../04_API/API_Specification.md`

---

**Document Status:** This is a living document and will be updated as requirements are clarified and implementation progresses.
