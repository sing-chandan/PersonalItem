# Implementation Roadmap & Progress Tracker
## Wholesale/Distributor Business Management SaaS

**Document Version:** 1.0
**Date:** 2026-08-29
**Status:** Living Document
**Project:** InventoryProc - Wholesale Distributor SaaS MVP

---

## Document Control

| Version | Date | Author | Changes | Status |
|---------|------|--------|---------|--------|
| 1.0 | 2026-08-29 | Development Team | Initial roadmap | 📝 Planning |

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Implementation Phases](#2-implementation-phases)
3. [Detailed Task Breakdown](#3-detailed-task-breakdown)
4. [Dependency Graph](#4-dependency-graph)
5. [Testing Strategy](#5-testing-strategy)
6. [Deployment Plan](#6-deployment-plan)
7. [Progress Tracking](#7-progress-tracking)
8. [Risks & Mitigation](#8-risks--mitigation)

---

## 1. Project Overview

### 1.1 Project Goals
- Build MVP of Wholesale/Distributor Management SaaS
- Support core business operations: Products, Customers, Suppliers, Inventory, Sales, Purchases, Payments
- Multi-tenant architecture from day one
- Excel import capability
- Comprehensive reporting

### 1.2 Timeline
- **Target MVP Completion:** 12-16 weeks
- **Alpha Release:** Week 10
- **Beta Release:** Week 14
- **Production Launch:** Week 16

### 1.3 Team Structure
- Backend Developers: 2-3
- Frontend Developers: 2 (Angular + Flutter)
- QA/Testing: 1
- DevOps: 1 (part-time)
- Product Owner: 1

### 1.4 Success Criteria
✓ All MVP features functional
✓ Multi-tenant isolation verified
✓ Both SQL Server and PostgreSQL tested
✓ 80%+ test coverage
✓ Demo data available
✓ Documentation complete
✓ 5+ pilot tenants onboarded

---

## 2. Implementation Phases

### Phase 1: Foundation (Weeks 1-3)
**Status:** 🟡 Not Started

**Objectives:**
- Set up development environment
- Create project structure
- Implement authentication & multi-tenancy
- Database schema & migrations

**Deliverables:**
- ✅ Project scaffolding (Backend + Frontend)
- ✅ Database migrations (SQL Server & PostgreSQL)
- ✅ Authentication system (JWT)
- ✅ Tenant management
- ✅ User management
- ✅ Global query filters
- ✅ API infrastructure (middleware, error handling)

---

### Phase 2: Master Data (Weeks 4-5)
**Status:** 🟡 Not Started

**Objectives:**
- Implement core master data modules
- CRUD operations for Products, Customers, Suppliers
- Categories, Brands, Units

**Deliverables:**
- ✅ Products module (full CRUD)
- ✅ Categories, Brands, Units
- ✅ Customers module (full CRUD)
- ✅ Suppliers module (full CRUD)
- ✅ Search & filtering
- ✅ Pagination
- ✅ Angular components for master data
- ✅ Unit tests for master data

---

### Phase 3: Inventory Management (Weeks 6-7)
**Status:** 🟡 Not Started

**Objectives:**
- Stock balance tracking
- Inventory transactions
- Opening stock entry

**Deliverables:**
- ✅ Stock balance entity & logic
- ✅ Inventory transactions
- ✅ Opening stock entry
- ✅ Stock validation logic
- ✅ Inventory service contracts
- ✅ Stock reports (current stock, low stock)
- ✅ Angular inventory components
- ✅ Integration tests for inventory

---

### Phase 4: Purchases (Week 8)
**Status:** 🟡 Not Started

**Objectives:**
- Purchase creation with line items
- Automatic inventory update
- Purchase-inventory integration

**Deliverables:**
- ✅ Purchase module (create, list, view)
- ✅ Purchase items management
- ✅ Inventory integration (stock increase)
- ✅ Purchase API endpoints
- ✅ Angular purchase components
- ✅ Transaction rollback handling
- ✅ Purchase module tests

---

### Phase 5: Sales & Billing (Weeks 9-10)
**Status:** 🟡 Not Started

**Objectives:**
- Sales invoice creation
- Stock validation & deduction
- Customer ledger integration
- Invoice generation

**Deliverables:**
- ✅ Sales module (create, list, view)
- ✅ Sale items management
- ✅ Stock validation before sale
- ✅ Inventory integration (stock decrease)
- ✅ Customer ledger creation
- ✅ Payment at time of sale
- ✅ Invoice generation (HTML/PDF)
- ✅ Angular sales components
- ✅ Sales module tests

---

### Phase 6: Payments & Ledger (Week 11)
**Status:** 🟡 Not Started

**Objectives:**
- Payment recording
- Customer ledger management
- Outstanding balance tracking

**Deliverables:**
- ✅ Payments module (create, list, view)
- ✅ Customer ledger service
- ✅ Outstanding balance calculation
- ✅ Customer statement generation
- ✅ Payment-ledger integration
- ✅ Angular payment components
- ✅ Payment module tests

---

### Phase 7: Reporting (Week 12)
**Status:** 🟡 Not Started

**Objectives:**
- Dashboard with key metrics
- Sales, Purchase, Inventory reports
- Customer outstanding reports

**Deliverables:**
- ✅ Dashboard API & UI
- ✅ Sales reports (summary, by product, by customer)
- ✅ Purchase reports (summary, by supplier)
- ✅ Inventory reports (current stock, low stock, movements)
- ✅ Customer outstanding reports
- ✅ Excel/PDF export
- ✅ Report caching (if needed)
- ✅ Angular reporting components

---

### Phase 8: Excel Import (Week 13)
**Status:** 🟡 Not Started

**Objectives:**
- Excel import for master data & opening stock
- Validation & preview
- Error reporting

**Deliverables:**
- ✅ Excel template downloads
- ✅ File upload & parsing
- ✅ Data validation
- ✅ Preview before import
- ✅ Import execution
- ✅ Error reporting with line numbers
- ✅ Import for: Products, Customers, Suppliers, Opening Stock
- ✅ Angular import components
- ✅ Import tests

---

### Phase 9: Flutter Mobile App (Weeks 14-15)
**Status:** 🟡 Not Started

**Objectives:**
- Flutter app for focused workflows
- Customer lookup, Sales creation, Payment recording
- Offline capability (optional MVP)

**Deliverables:**
- ✅ Flutter project setup
- ✅ Authentication & API integration
- ✅ Dashboard screen
- ✅ Customer list & detail screens
- ✅ Product search screen
- ✅ Sales creation screen
- ✅ Payment recording screen
- ✅ Outstanding balance view
- ✅ Flutter app testing

---

### Phase 10: Production Hardening (Week 16)
**Status:** 🟡 Not Started

**Objectives:**
- Security hardening
- Performance optimization
- Production deployment
- Monitoring & logging

**Deliverables:**
- ✅ Security audit & fixes
- ✅ Performance testing & optimization
- ✅ Tenant isolation verification tests
- ✅ SQL Server & PostgreSQL testing
- ✅ Docker configuration
- ✅ CI/CD pipeline
- ✅ Production deployment scripts
- ✅ Backup & restore procedures
- ✅ Monitoring & alerting setup
- ✅ Demo data seed scripts
- ✅ User documentation
- ✅ API documentation (Swagger)

---

## 3. Detailed Task Breakdown

### 3.1 Phase 1: Foundation - Detailed Tasks

#### Backend Infrastructure
- [ ] **Task 1.1:** Create ASP.NET Core solution structure
  - Duration: 1 day
  - Dependencies: None
  - Status: 🟡 Not Started

- [ ] **Task 1.2:** Set up EF Core with SQL Server & PostgreSQL
  - Duration: 2 days
  - Dependencies: 1.1
  - Status: 🟡 Not Started

- [ ] **Task 1.3:** Create database schema & initial migrations
  - Duration: 2 days
  - Dependencies: 1.2
  - Status: 🟡 Not Started

- [ ] **Task 1.4:** Implement Tenant entity & management
  - Duration: 2 days
  - Dependencies: 1.3
  - Status: 🟡 Not Started

- [ ] **Task 1.5:** Implement User entity & ASP.NET Identity
  - Duration: 2 days
  - Dependencies: 1.4
  - Status: 🟡 Not Started

- [ ] **Task 1.6:** Implement JWT authentication
  - Duration: 2 days
  - Dependencies: 1.5
  - Status: 🟡 Not Started

- [ ] **Task 1.7:** Implement tenant resolution middleware
  - Duration: 1 day
  - Dependencies: 1.6
  - Status: 🟡 Not Started

- [ ] **Task 1.8:** Implement global query filters for multi-tenancy
  - Duration: 2 days
  - Dependencies: 1.7
  - Status: 🟡 Not Started

- [ ] **Task 1.9:** Implement exception handling middleware
  - Duration: 1 day
  - Dependencies: 1.6
  - Status: 🟡 Not Started

- [ ] **Task 1.10:** Set up logging (Serilog)
  - Duration: 1 day
  - Dependencies: 1.1
  - Status: 🟡 Not Started

- [ ] **Task 1.11:** Set up Swagger/OpenAPI
  - Duration: 0.5 days
  - Dependencies: 1.1
  - Status: 🟡 Not Started

- [ ] **Task 1.12:** Set up FluentValidation
  - Duration: 0.5 days
  - Dependencies: 1.1
  - Status: 🟡 Not Started

- [ ] **Task 1.13:** Set up AutoMapper
  - Duration: 0.5 days
  - Dependencies: 1.1
  - Status: 🟡 Not Started

- [ ] **Task 1.14:** Write unit tests for authentication
  - Duration: 2 days
  - Dependencies: 1.6
  - Status: 🟡 Not Started

- [ ] **Task 1.15:** Write integration tests for tenant isolation
  - Duration: 2 days
  - Dependencies: 1.8
  - Status: 🟡 Not Started

#### Frontend Infrastructure
- [ ] **Task 1.16:** Create Angular project structure
  - Duration: 1 day
  - Dependencies: None
  - Status: 🟡 Not Started

- [ ] **Task 1.17:** Set up Angular Material / PrimeNG
  - Duration: 1 day
  - Dependencies: 1.16
  - Status: 🟡 Not Started

- [ ] **Task 1.18:** Create authentication service & guards
  - Duration: 2 days
  - Dependencies: 1.17
  - Status: 🟡 Not Started

- [ ] **Task 1.19:** Create login & registration components
  - Duration: 2 days
  - Dependencies: 1.18
  - Status: 🟡 Not Started

- [ ] **Task 1.20:** Create main layout (header, sidebar, content)
  - Duration: 2 days
  - Dependencies: 1.18
  - Status: 🟡 Not Started

- [ ] **Task 1.21:** Set up HTTP interceptors (auth, error)
  - Duration: 1 day
  - Dependencies: 1.18
  - Status: 🟡 Not Started

- [ ] **Task 1.22:** Create shared components (data-table, search, etc.)
  - Duration: 2 days
  - Dependencies: 1.17
  - Status: 🟡 Not Started

---

### 3.2 Phase 2: Master Data - Detailed Tasks

#### Products Module
- [ ] **Task 2.1:** Create Product entity & configuration
  - Duration: 0.5 days
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 2.2:** Create Product repository
  - Duration: 0.5 days
  - Dependencies: 2.1
  - Status: 🟡 Not Started

- [ ] **Task 2.3:** Create Product service & DTOs
  - Duration: 1 day
  - Dependencies: 2.2
  - Status: 🟡 Not Started

- [ ] **Task 2.4:** Create Product API endpoints (CRUD)
  - Duration: 1 day
  - Dependencies: 2.3
  - Status: 🟡 Not Started

- [ ] **Task 2.5:** Create Product validators
  - Duration: 0.5 days
  - Dependencies: 2.3
  - Status: 🟡 Not Started

- [ ] **Task 2.6:** Create Angular Product components
  - Duration: 2 days
  - Dependencies: 2.4
  - Status: 🟡 Not Started

- [ ] **Task 2.7:** Write Product module tests
  - Duration: 1 day
  - Dependencies: 2.4
  - Status: 🟡 Not Started

#### Categories, Brands, Units
- [ ] **Task 2.8:** Create Category entity & CRUD
  - Duration: 1 day
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 2.9:** Create Brand entity & CRUD
  - Duration: 1 day
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 2.10:** Create Unit entity & CRUD
  - Duration: 1 day
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 2.11:** Create Angular components for Category, Brand, Unit
  - Duration: 2 days
  - Dependencies: 2.8, 2.9, 2.10
  - Status: 🟡 Not Started

#### Customers Module
- [ ] **Task 2.12:** Create Customer entity & configuration
  - Duration: 0.5 days
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 2.13:** Create Customer repository & service
  - Duration: 1 day
  - Dependencies: 2.12
  - Status: 🟡 Not Started

- [ ] **Task 2.14:** Create Customer API endpoints (CRUD)
  - Duration: 1 day
  - Dependencies: 2.13
  - Status: 🟡 Not Started

- [ ] **Task 2.15:** Create Angular Customer components
  - Duration: 2 days
  - Dependencies: 2.14
  - Status: 🟡 Not Started

- [ ] **Task 2.16:** Write Customer module tests
  - Duration: 1 day
  - Dependencies: 2.14
  - Status: 🟡 Not Started

#### Suppliers Module
- [ ] **Task 2.17:** Create Supplier entity & CRUD (similar to Customer)
  - Duration: 2 days
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 2.18:** Create Angular Supplier components
  - Duration: 2 days
  - Dependencies: 2.17
  - Status: 🟡 Not Started

- [ ] **Task 2.19:** Write Supplier module tests
  - Duration: 1 day
  - Dependencies: 2.17
  - Status: 🟡 Not Started

---

### 3.3 Phase 3: Inventory - Detailed Tasks

- [ ] **Task 3.1:** Create StockBalance entity & configuration
  - Duration: 0.5 days
  - Dependencies: Phase 2 (Products)
  - Status: 🟡 Not Started

- [ ] **Task 3.2:** Create InventoryTransaction entity
  - Duration: 0.5 days
  - Dependencies: 3.1
  - Status: 🟡 Not Started

- [ ] **Task 3.3:** Create Inventory service with contracts
  - Duration: 2 days
  - Dependencies: 3.2
  - Status: 🟡 Not Started

- [ ] **Task 3.4:** Implement stock increase/decrease logic
  - Duration: 1 day
  - Dependencies: 3.3
  - Status: 🟡 Not Started

- [ ] **Task 3.5:** Implement stock validation logic
  - Duration: 1 day
  - Dependencies: 3.4
  - Status: 🟡 Not Started

- [ ] **Task 3.6:** Create opening stock API endpoint
  - Duration: 1 day
  - Dependencies: 3.4
  - Status: 🟡 Not Started

- [ ] **Task 3.7:** Create stock adjustment API endpoint
  - Duration: 1 day
  - Dependencies: 3.4
  - Status: 🟡 Not Started

- [ ] **Task 3.8:** Create inventory query endpoints (stock, transactions)
  - Duration: 1 day
  - Dependencies: 3.3
  - Status: 🟡 Not Started

- [ ] **Task 3.9:** Create Angular inventory components
  - Duration: 2 days
  - Dependencies: 3.8
  - Status: 🟡 Not Started

- [ ] **Task 3.10:** Write inventory module tests (unit & integration)
  - Duration: 2 days
  - Dependencies: 3.4, 3.5
  - Status: 🟡 Not Started

---

### 3.4 Phase 4: Purchases - Detailed Tasks

- [ ] **Task 4.1:** Create Purchase & PurchaseItem entities
  - Duration: 1 day
  - Dependencies: Phase 2 (Products, Suppliers)
  - Status: 🟡 Not Started

- [ ] **Task 4.2:** Create Purchase repository
  - Duration: 0.5 days
  - Dependencies: 4.1
  - Status: 🟡 Not Started

- [ ] **Task 4.3:** Create Purchase service with business logic
  - Duration: 2 days
  - Dependencies: 4.2, Phase 3 (Inventory)
  - Status: 🟡 Not Started

- [ ] **Task 4.4:** Implement Purchase creation with inventory update
  - Duration: 2 days
  - Dependencies: 4.3
  - Status: 🟡 Not Started

- [ ] **Task 4.5:** Implement server-side total calculation
  - Duration: 0.5 days
  - Dependencies: 4.3
  - Status: 🟡 Not Started

- [ ] **Task 4.6:** Create Purchase API endpoints
  - Duration: 1 day
  - Dependencies: 4.4
  - Status: 🟡 Not Started

- [ ] **Task 4.7:** Create Angular Purchase form component
  - Duration: 2 days
  - Dependencies: 4.6
  - Status: 🟡 Not Started

- [ ] **Task 4.8:** Create Angular Purchase list & detail components
  - Duration: 1 day
  - Dependencies: 4.6
  - Status: 🟡 Not Started

- [ ] **Task 4.9:** Write Purchase module tests (transaction rollback)
  - Duration: 2 days
  - Dependencies: 4.4
  - Status: 🟡 Not Started

---

### 3.5 Phase 5: Sales - Detailed Tasks

- [ ] **Task 5.1:** Create Sale & SaleItem entities
  - Duration: 1 day
  - Dependencies: Phase 2 (Products, Customers)
  - Status: 🟡 Not Started

- [ ] **Task 5.2:** Create CustomerLedgerEntry entity
  - Duration: 0.5 days
  - Dependencies: Phase 2 (Customers)
  - Status: 🟡 Not Started

- [ ] **Task 5.3:** Create Sale repository
  - Duration: 0.5 days
  - Dependencies: 5.1
  - Status: 🟡 Not Started

- [ ] **Task 5.4:** Create Customer Ledger service
  - Duration: 1 day
  - Dependencies: 5.2
  - Status: 🟡 Not Started

- [ ] **Task 5.5:** Create Sale service with complex business logic
  - Duration: 3 days
  - Dependencies: 5.3, 5.4, Phase 3 (Inventory)
  - Status: 🟡 Not Started

- [ ] **Task 5.6:** Implement stock validation before sale
  - Duration: 1 day
  - Dependencies: 5.5
  - Status: 🟡 Not Started

- [ ] **Task 5.7:** Implement Sale creation with inventory & ledger update
  - Duration: 2 days
  - Dependencies: 5.6
  - Status: 🟡 Not Started

- [ ] **Task 5.8:** Implement payment status calculation
  - Duration: 0.5 days
  - Dependencies: 5.5
  - Status: 🟡 Not Started

- [ ] **Task 5.9:** Create Sale API endpoints
  - Duration: 1 day
  - Dependencies: 5.7
  - Status: 🟡 Not Started

- [ ] **Task 5.10:** Create invoice generation service (HTML/PDF)
  - Duration: 2 days
  - Dependencies: 5.9
  - Status: 🟡 Not Started

- [ ] **Task 5.11:** Create Angular Sales form component
  - Duration: 3 days
  - Dependencies: 5.9
  - Status: 🟡 Not Started

- [ ] **Task 5.12:** Create Angular Sales list & detail components
  - Duration: 1 day
  - Dependencies: 5.9
  - Status: 🟡 Not Started

- [ ] **Task 5.13:** Create Angular Invoice view component
  - Duration: 1 day
  - Dependencies: 5.10
  - Status: 🟡 Not Started

- [ ] **Task 5.14:** Write Sales module tests (complex scenarios)
  - Duration: 3 days
  - Dependencies: 5.7
  - Status: 🟡 Not Started

---

### 3.6 Phase 6: Payments - Detailed Tasks

- [ ] **Task 6.1:** Create Payment entity
  - Duration: 0.5 days
  - Dependencies: Phase 2 (Customers)
  - Status: 🟡 Not Started

- [ ] **Task 6.2:** Create Payment repository & service
  - Duration: 1 day
  - Dependencies: 6.1, Phase 5 (Ledger)
  - Status: 🟡 Not Started

- [ ] **Task 6.3:** Implement payment recording with ledger update
  - Duration: 2 days
  - Dependencies: 6.2
  - Status: 🟡 Not Started

- [ ] **Task 6.4:** Implement outstanding balance calculation
  - Duration: 1 day
  - Dependencies: 6.3
  - Status: 🟡 Not Started

- [ ] **Task 6.5:** Create customer statement generation
  - Duration: 1 day
  - Dependencies: 6.4
  - Status: 🟡 Not Started

- [ ] **Task 6.6:** Create Payment API endpoints
  - Duration: 1 day
  - Dependencies: 6.3
  - Status: 🟡 Not Started

- [ ] **Task 6.7:** Create Customer ledger API endpoints
  - Duration: 1 day
  - Dependencies: 6.4
  - Status: 🟡 Not Started

- [ ] **Task 6.8:** Create Angular Payment components
  - Duration: 2 days
  - Dependencies: 6.6
  - Status: 🟡 Not Started

- [ ] **Task 6.9:** Create Angular Ledger view component
  - Duration: 1 day
  - Dependencies: 6.7
  - Status: 🟡 Not Started

- [ ] **Task 6.10:** Write Payment module tests
  - Duration: 2 days
  - Dependencies: 6.3
  - Status: 🟡 Not Started

---

### 3.7 Phase 7: Reporting - Detailed Tasks

- [ ] **Task 7.1:** Create Dashboard query service
  - Duration: 2 days
  - Dependencies: Phase 5 (Sales), Phase 6 (Payments)
  - Status: 🟡 Not Started

- [ ] **Task 7.2:** Create Dashboard API endpoint
  - Duration: 1 day
  - Dependencies: 7.1
  - Status: 🟡 Not Started

- [ ] **Task 7.3:** Create Sales report query services
  - Duration: 2 days
  - Dependencies: Phase 5
  - Status: 🟡 Not Started

- [ ] **Task 7.4:** Create Sales report API endpoints
  - Duration: 1 day
  - Dependencies: 7.3
  - Status: 🟡 Not Started

- [ ] **Task 7.5:** Create Purchase report query services
  - Duration: 1 day
  - Dependencies: Phase 4
  - Status: 🟡 Not Started

- [ ] **Task 7.6:** Create Purchase report API endpoints
  - Duration: 1 day
  - Dependencies: 7.5
  - Status: 🟡 Not Started

- [ ] **Task 7.7:** Create Inventory report query services
  - Duration: 1 day
  - Dependencies: Phase 3
  - Status: 🟡 Not Started

- [ ] **Task 7.8:** Create Inventory report API endpoints
  - Duration: 1 day
  - Dependencies: 7.7
  - Status: 🟡 Not Started

- [ ] **Task 7.9:** Create Customer outstanding report query service
  - Duration: 1 day
  - Dependencies: Phase 6
  - Status: 🟡 Not Started

- [ ] **Task 7.10:** Create Customer outstanding report API endpoint
  - Duration: 1 day
  - Dependencies: 7.9
  - Status: 🟡 Not Started

- [ ] **Task 7.11:** Implement Excel export service
  - Duration: 2 days
  - Dependencies: 7.1-7.10
  - Status: 🟡 Not Started

- [ ] **Task 7.12:** Implement PDF export service (optional)
  - Duration: 2 days
  - Dependencies: 7.1-7.10
  - Status: 🟡 Not Started

- [ ] **Task 7.13:** Create Angular Dashboard component
  - Duration: 2 days
  - Dependencies: 7.2
  - Status: 🟡 Not Started

- [ ] **Task 7.14:** Create Angular Sales report components
  - Duration: 2 days
  - Dependencies: 7.4
  - Status: 🟡 Not Started

- [ ] **Task 7.15:** Create Angular Inventory report components
  - Duration: 1 day
  - Dependencies: 7.8
  - Status: 🟡 Not Started

- [ ] **Task 7.16:** Create Angular Outstanding report components
  - Duration: 1 day
  - Dependencies: 7.10
  - Status: 🟡 Not Started

- [ ] **Task 7.17:** Optimize report queries (indexing, caching)
  - Duration: 2 days
  - Dependencies: 7.1-7.10
  - Status: 🟡 Not Started

---

### 3.8 Phase 8: Excel Import - Detailed Tasks

- [ ] **Task 8.1:** Set up Excel library (EPPlus/ClosedXML)
  - Duration: 0.5 days
  - Dependencies: Phase 1
  - Status: 🟡 Not Started

- [ ] **Task 8.2:** Create Excel template generator
  - Duration: 1 day
  - Dependencies: 8.1
  - Status: 🟡 Not Started

- [ ] **Task 8.3:** Create Excel parser service
  - Duration: 1 day
  - Dependencies: 8.1
  - Status: 🟡 Not Started

- [ ] **Task 8.4:** Create import validation service
  - Duration: 2 days
  - Dependencies: 8.3
  - Status: 🟡 Not Started

- [ ] **Task 8.5:** Implement Product import
  - Duration: 2 days
  - Dependencies: 8.4, Phase 2
  - Status: 🟡 Not Started

- [ ] **Task 8.6:** Implement Customer import
  - Duration: 2 days
  - Dependencies: 8.4, Phase 2
  - Status: 🟡 Not Started

- [ ] **Task 8.7:** Implement Supplier import
  - Duration: 2 days
  - Dependencies: 8.4, Phase 2
  - Status: 🟡 Not Started

- [ ] **Task 8.8:** Implement Opening Stock import
  - Duration: 2 days
  - Dependencies: 8.4, Phase 3
  - Status: 🟡 Not Started

- [ ] **Task 8.9:** Create Import API endpoints (upload, status, templates)
  - Duration: 1 day
  - Dependencies: 8.5-8.8
  - Status: 🟡 Not Started

- [ ] **Task 8.10:** Create Angular Import components with preview
  - Duration: 3 days
  - Dependencies: 8.9
  - Status: 🟡 Not Started

- [ ] **Task 8.11:** Write Import module tests (validation scenarios)
  - Duration: 2 days
  - Dependencies: 8.5-8.8
  - Status: 🟡 Not Started

---

### 3.9 Phase 9: Flutter Mobile - Detailed Tasks

- [ ] **Task 9.1:** Create Flutter project structure
  - Duration: 1 day
  - Dependencies: Phase 1 (API)
  - Status: 🟡 Not Started

- [ ] **Task 9.2:** Set up API client & authentication
  - Duration: 2 days
  - Dependencies: 9.1
  - Status: 🟡 Not Started

- [ ] **Task 9.3:** Create login screen
  - Duration: 1 day
  - Dependencies: 9.2
  - Status: 🟡 Not Started

- [ ] **Task 9.4:** Create Dashboard screen
  - Duration: 2 days
  - Dependencies: 9.2, Phase 7
  - Status: 🟡 Not Started

- [ ] **Task 9.5:** Create Customer list & search screens
  - Duration: 2 days
  - Dependencies: 9.2, Phase 2
  - Status: 🟡 Not Started

- [ ] **Task 9.6:** Create Product search screen
  - Duration: 2 days
  - Dependencies: 9.2, Phase 2
  - Status: 🟡 Not Started

- [ ] **Task 9.7:** Create Sales creation screen
  - Duration: 3 days
  - Dependencies: 9.5, 9.6, Phase 5
  - Status: 🟡 Not Started

- [ ] **Task 9.8:** Create Payment recording screen
  - Duration: 2 days
  - Dependencies: 9.5, Phase 6
  - Status: 🟡 Not Started

- [ ] **Task 9.9:** Create Outstanding balance screen
  - Duration: 1 day
  - Dependencies: 9.5, Phase 6
  - Status: 🟡 Not Started

- [ ] **Task 9.10:** Implement state management (Provider/Riverpod)
  - Duration: 2 days
  - Dependencies: 9.3-9.9
  - Status: 🟡 Not Started

- [ ] **Task 9.11:** Test Flutter app on Android & iOS
  - Duration: 2 days
  - Dependencies: 9.3-9.9
  - Status: 🟡 Not Started

---

### 3.10 Phase 10: Production Hardening - Detailed Tasks

- [ ] **Task 10.1:** Security audit (OWASP checklist)
  - Duration: 2 days
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.2:** Tenant isolation verification tests
  - Duration: 2 days
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.3:** SQL Server integration tests
  - Duration: 1 day
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.4:** PostgreSQL integration tests
  - Duration: 1 day
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.5:** Performance testing & profiling
  - Duration: 2 days
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.6:** Query optimization (indexes, caching)
  - Duration: 2 days
  - Dependencies: 10.5
  - Status: 🟡 Not Started

- [ ] **Task 10.7:** Create Docker configuration
  - Duration: 1 day
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.8:** Set up CI/CD pipeline (GitHub Actions/Azure DevOps)
  - Duration: 2 days
  - Dependencies: 10.7
  - Status: 🟡 Not Started

- [ ] **Task 10.9:** Create production deployment scripts
  - Duration: 1 day
  - Dependencies: 10.7, 10.8
  - Status: 🟡 Not Started

- [ ] **Task 10.10:** Set up database backup & restore
  - Duration: 1 day
  - Dependencies: 10.9
  - Status: 🟡 Not Started

- [ ] **Task 10.11:** Set up monitoring & logging (Application Insights)
  - Duration: 2 days
  - Dependencies: 10.9
  - Status: 🟡 Not Started

- [ ] **Task 10.12:** Create demo data seed scripts
  - Duration: 2 days
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.13:** Write user documentation
  - Duration: 3 days
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.14:** API documentation (Swagger) review & polish
  - Duration: 1 day
  - Dependencies: All phases
  - Status: 🟡 Not Started

- [ ] **Task 10.15:** Production deployment dry run
  - Duration: 1 day
  - Dependencies: 10.9
  - Status: 🟡 Not Started

- [ ] **Task 10.16:** Production deployment
  - Duration: 1 day
  - Dependencies: 10.15
  - Status: 🟡 Not Started

---

## 4. Dependency Graph

```
Phase 1 (Foundation)
    │
    ├──────────────┬──────────────┬──────────────┐
    ↓              ↓              ↓              ↓
Phase 2        Phase 2        Phase 2        Phase 7
(Products)   (Customers)   (Suppliers)    (Dashboard)
    │              │              │
    └──────┬───────┴──────────────┘
           │
           ↓
      Phase 3
    (Inventory)
           │
           ├──────────────┬──────────────┐
           ↓              ↓              │
      Phase 4        Phase 5             │
    (Purchases)     (Sales)              │
           │              │              │
           │              ↓              │
           │         Phase 6             │
           │       (Payments)            │
           │              │              │
           └──────┬───────┴──────────────┘
                  │
                  ↓
             Phase 7
           (Reporting)
                  │
                  ├──────────────┬──────────────┐
                  ↓              ↓              ↓
             Phase 8        Phase 9        Phase 10
           (Imports)      (Flutter)     (Hardening)
```

---

## 5. Testing Strategy

### 5.1 Unit Tests
**Coverage Target:** 80%+

**Scope:**
- Domain entities & business logic
- Application services
- Validators
- Calculations (totals, balances)

**Tools:** xUnit, Moq, FluentAssertions

### 5.2 Integration Tests
**Scope:**
- API endpoints (all CRUD operations)
- Database operations
- Multi-tenancy isolation
- Transaction rollback scenarios
- Both SQL Server & PostgreSQL

**Tools:** xUnit, WebApplicationFactory, Testcontainers

### 5.3 End-to-End Tests
**Scope:**
- Complete business workflows:
  - User registration → Login → Create Product → Create Sale
  - Purchase → Inventory Update verification
  - Sale → Stock Deduction → Ledger Update
  - Payment → Outstanding Update

**Tools:** Playwright or Selenium (optional for MVP)

### 5.4 Security Tests
**Scope:**
- Cross-tenant access attempts (must fail)
- Authentication bypass attempts
- SQL injection attempts
- XSS attempts

**Tools:** OWASP ZAP (optional), manual testing

### 5.5 Performance Tests
**Scope:**
- API response times under load
- Report generation performance
- Concurrent user simulation

**Tools:** k6 or JMeter (optional)

---

## 6. Deployment Plan

### 6.1 Development Environment
- **Database:** SQL Server (LocalDB or Docker)
- **Backend:** ASP.NET Core (Kestrel, port 5000)
- **Frontend:** Angular Dev Server (port 4200)
- **Tools:** Visual Studio, VS Code, Postman

### 6.2 Staging Environment
- **Hosting:** Azure App Service / AWS / DigitalOcean
- **Database:** PostgreSQL (managed service)
- **Backend:** Dockerized ASP.NET Core
- **Frontend:** Dockerized Angular (Nginx)
- **URL:** https://staging.inventoryproc.com

### 6.3 Production Environment
- **Hosting:** Azure / AWS / DigitalOcean (scalable)
- **Database:** PostgreSQL with daily backups
- **Backend:** Dockerized with auto-scaling
- **Frontend:** CDN + Dockerized Nginx
- **URL:** https://app.inventoryproc.com
- **Monitoring:** Application Insights / Datadog
- **Logging:** Serilog → Seq / ELK Stack

### 6.4 CI/CD Pipeline
```
Code Push (GitHub)
    │
    ↓
GitHub Actions / Azure DevOps
    │
    ├─ Build Backend
    ├─ Run Tests
    ├─ Build Docker Image
    │
    ├─ Build Frontend
    ├─ Run Frontend Tests
    ├─ Build Docker Image
    │
    ↓
Push to Container Registry
    │
    ↓
Deploy to Environment
    │
    ├─ Development (auto)
    ├─ Staging (auto on main branch)
    └─ Production (manual approval)
```

---

## 7. Progress Tracking

### 7.1 Weekly Review Checklist

**Every Monday:**
- [ ] Review last week's completed tasks
- [ ] Update task statuses
- [ ] Identify blockers
- [ ] Plan current week's tasks
- [ ] Update burn-down chart

### 7.2 Status Indicators

| Symbol | Status | Description |
|--------|--------|-------------|
| 🟡 | Not Started | Task not yet begun |
| 🔵 | In Progress | Currently being worked on |
| 🟢 | Completed | Task finished & tested |
| 🔴 | Blocked | Waiting on dependency or issue |
| ⚠️ | At Risk | Behind schedule or issues found |

### 7.3 Progress Dashboard

**Overall Progress:** 0% (0/200+ tasks)

**Phase Progress:**
- Phase 1 (Foundation): 0% (0/22 tasks)
- Phase 2 (Master Data): 0% (0/19 tasks)
- Phase 3 (Inventory): 0% (0/10 tasks)
- Phase 4 (Purchases): 0% (0/9 tasks)
- Phase 5 (Sales): 0% (0/14 tasks)
- Phase 6 (Payments): 0% (0/10 tasks)
- Phase 7 (Reporting): 0% (0/17 tasks)
- Phase 8 (Imports): 0% (0/11 tasks)
- Phase 9 (Flutter): 0% (0/11 tasks)
- Phase 10 (Hardening): 0% (0/16 tasks)

### 7.4 Milestone Tracking

| Milestone | Target Date | Status | Notes |
|-----------|-------------|--------|-------|
| Foundation Complete | Week 3 | 🟡 | - |
| Master Data Complete | Week 5 | 🟡 | - |
| Inventory Complete | Week 7 | 🟡 | - |
| Transactions Complete (Purchase+Sale) | Week 10 | 🟡 | - |
| Payments & Reporting Complete | Week 12 | 🟡 | - |
| Imports Complete | Week 13 | 🟡 | - |
| Flutter MVP Complete | Week 15 | 🟡 | - |
| Production Ready | Week 16 | 🟡 | - |
| First Pilot Tenant Onboarded | Week 17 | 🟡 | - |

---

## 8. Risks & Mitigation

### Risk 1: Database Portability Issues
**Probability:** Medium
**Impact:** High
**Mitigation:**
- Use only EF Core portable features
- Test on both SQL Server and PostgreSQL regularly
- Avoid database-specific SQL in business logic
- Maintain dual migrations from day one

### Risk 2: Complex Transaction Handling
**Probability:** Medium
**Impact:** High
**Mitigation:**
- Design transactions carefully (Purchase, Sale, Payment)
- Write comprehensive rollback tests
- Use database constraints to enforce integrity
- Test failure scenarios explicitly

### Risk 3: Multi-Tenant Data Leakage
**Probability:** Low
**Impact:** Critical
**Mitigation:**
- Implement global query filters from day one
- Write cross-tenant access tests for every feature
- Security audit before production
- Regular penetration testing

### Risk 4: Performance Issues with Reports
**Probability:** Medium
**Impact:** Medium
**Mitigation:**
- Design indexes strategically
- Optimize queries early
- Use query projections (not full entities)
- Consider caching for dashboard
- Load test with realistic data volumes

### Risk 5: Scope Creep
**Probability:** High
**Impact:** Medium
**Mitigation:**
- Stick to MVP features
- Document "out of scope" items
- Use Phase 2 document as contract
- Get approval for any new features

### Risk 6: Integration Complexity (Angular + .NET + Flutter)
**Probability:** Medium
**Impact:** Medium
**Mitigation:**
- Define clear API contracts
- Use DTOs consistently
- Automate API testing
- Keep API versioning in mind

### Risk 7: Excel Import Edge Cases
**Probability:** High
**Impact:** Low
**Mitigation:**
- Comprehensive validation before import
- Preview mechanism for users
- Detailed error reporting with line numbers
- Atomic transactions (all or nothing)

### Risk 8: Timeline Slippage
**Probability:** Medium
**Impact:** Medium
**Mitigation:**
- Buffer time in estimates (20%)
- Weekly progress reviews
- Identify blockers early
- Adjust scope if needed (cut non-critical features)

---

## 9. Success Metrics

### 9.1 Technical Metrics
- ✅ 80%+ unit test coverage
- ✅ 100% critical path integration tests
- ✅ Zero cross-tenant data leakage
- ✅ API response time < 1 second (90th percentile)
- ✅ Zero SQL injection vulnerabilities
- ✅ Both SQL Server and PostgreSQL tested

### 9.2 Business Metrics
- ✅ 5+ pilot tenants onboarded
- ✅ 90%+ user satisfaction (from pilots)
- ✅ < 5 critical bugs post-launch
- ✅ < 2 hours downtime in first month
- ✅ Complete documentation available

### 9.3 Feature Completeness
- ✅ All MVP features implemented
- ✅ Excel import for 4 entities (Products, Customers, Suppliers, Opening Stock)
- ✅ 10+ reports functional
- ✅ Invoice generation working
- ✅ Flutter app with core features

---

## Appendix A: Change Log

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-08-29 | 1.0 | Initial roadmap created | Development Team |

---

## Appendix B: Team Contacts

| Role | Name | Email | Responsibility |
|------|------|-------|----------------|
| Product Owner | - | - | Requirements, priorities |
| Lead Backend Dev | - | - | .NET, EF Core, APIs |
| Backend Dev | - | - | Modules, business logic |
| Lead Frontend Dev | - | - | Angular architecture |
| Flutter Dev | - | - | Mobile app |
| QA Engineer | - | - | Testing, automation |
| DevOps Engineer | - | - | CI/CD, deployment |

---

**Document Status:** This is a living document. Update weekly with progress, blockers, and status changes.

**Next Review:** Week 1 - Project Kickoff
