# Database Schema & ER Diagram
## Wholesale/Distributor Business Management SaaS

**Document Version:** 1.0
**Date:** 2026-08-29
**Status:** Living Document
**Project:** InventoryProc - Wholesale Distributor SaaS MVP

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-08-29 | Development Team | Initial database schema |

---

## Table of Contents

1. [Database Overview](#1-database-overview)
2. [Entity Relationship Diagram](#2-entity-relationship-diagram)
3. [Table Definitions](#3-table-definitions)
4. [Indexes Strategy](#4-indexes-strategy)
5. [Database Constraints](#5-database-constraints)
6. [Migration Strategy](#6-migration-strategy)
7. [Data Seeding](#7-data-seeding)

---

## 1. Database Overview

### 1.1 Database Design Principles

1. **Multi-Tenancy:** All tenant-owned tables include `TenantId` for data isolation
2. **Audit Trail:** All transactional entities track `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
3. **Soft Delete:** Master data uses `IsActive` flag; no hard deletes
4. **Immutability:** Transaction records (Sales, Purchases, Payments) cannot be edited once created
5. **Referential Integrity:** Foreign keys enforce relationships at database level
6. **Database Portability:** Schema compatible with both SQL Server and PostgreSQL

### 1.2 Table Categories

| Category | Tables | Description |
|----------|--------|-------------|
| **Platform** | Tenants | Multi-tenant platform management |
| **Identity** | Users, Roles, UserRoles | Authentication and authorization |
| **Master Data** | Products, Categories, Brands, Units, Customers, Suppliers | Reference data |
| **Transactions** | Purchases, PurchaseItems, Sales, SaleItems, Payments | Business transactions |
| **Inventory** | StockBalances, InventoryTransactions | Stock management |
| **Ledger** | CustomerLedgerEntries | Financial tracking |

### 1.3 Database Statistics (Estimated)

| Entity | Estimated Rows (Small Tenant) | Estimated Rows (Large Tenant) |
|--------|-------------------------------|-------------------------------|
| Products | 500 | 100,000 |
| Customers | 100 | 10,000 |
| Suppliers | 20 | 1,000 |
| Sales | 1,000/month | 50,000/month |
| SaleItems | 5,000/month | 250,000/month |
| Purchases | 200/month | 10,000/month |
| InventoryTransactions | 6,000/month | 300,000/month |

---

## 2. Entity Relationship Diagram

### 2.1 High-Level ER Diagram

```
┌─────────────┐
│   Tenants   │
└──────┬──────┘
       │ 1
       │
       │ N
┌──────▼──────────────────────────────────────────────────────┐
│                    All Tenant-Owned Entities                 │
└──────────────────────────────────────────────────────────────┘
       │
       ├─────────────┬──────────────┬──────────────┐
       │             │              │              │
┌──────▼──────┐ ┌───▼────────┐ ┌──▼──────────┐ ┌─▼────────┐
│   Products  │ │  Customers │ │  Suppliers  │ │  Users   │
└──────┬──────┘ └─────┬──────┘ └──────┬──────┘ └──────────┘
       │              │                │
       │              │                │
       ├──────┬───────┼────────────────┤
       │      │       │                │
       │      │       │                │
┌──────▼──────▼───────▼─┐      ┌───────▼────────┐
│  InventoryTransactions│      │   Purchases    │
│    StockBalances      │      │ PurchaseItems  │
└───────────────────────┘      └────────────────┘
       │                               │
       │      ┌─────────────────┐      │
       │      │     Sales       │◄─────┘
       │      │   SaleItems     │
       │      └─────┬───────────┘
       │            │
       │      ┌─────▼───────────┐
       │      │   Payments      │
       │      │ CustomerLedger  │
       │      └─────────────────┘
       │
       └────────────┘
```

### 2.2 Detailed ER Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                            TENANTS                                    │
│  PK  Id (Guid)                                                        │
│      Code (string)                                                    │
│      Name (string)                                                    │
│      BusinessName (string)                                            │
│      Email, Phone, Address, GST, IsActive                            │
└───────────────────────────────┬──────────────────────────────────────┘
                                │
                                │ 1:N (TenantId)
                                │
┌───────────────────────────────┴──────────────────────────────────────┐
│                          TENANT-OWNED ENTITIES                        │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────┐         ┌──────────────────────┐
│     Categories       │         │       Brands         │
│ PK  Id               │         │ PK  Id               │
│ FK  TenantId         │         │ FK  TenantId         │
│     Name             │         │     Name             │
│     IsActive         │         │     IsActive         │
└──────┬───────────────┘         └──────┬───────────────┘
       │                                │
       │ 1:N                            │ 1:N
       │                                │
       └────────┬───────────────────────┘
                │
         ┌──────▼──────────────────────────────┐
         │          Products                   │
         │ PK  Id                              │
         │ FK  TenantId                        │
         │ FK  CategoryId                      │
         │ FK  BrandId                         │
         │ FK  UnitId                          │
         │     Name, SKU, Barcode              │
         │     PurchasePrice, SellingPrice     │
         │     MinimumStock, IsActive          │
         │     CreatedAt, UpdatedAt            │
         └──────┬──────────────────────────────┘
                │
                │ 1:N
                │
         ┌──────▼──────────────────────────────┐
         │      StockBalances                  │
         │ PK  Id                              │
         │ FK  TenantId                        │
         │ FK  ProductId                       │
         │     QuantityOnHand                  │
         │     ReservedQuantity                │
         │     UpdatedAt                       │
         │ UQ  (TenantId, ProductId)           │
         └─────────────────────────────────────┘

         ┌──────────────────────────────────────┐
         │    InventoryTransactions             │
         │ PK  Id                               │
         │ FK  TenantId                         │
         │ FK  ProductId                        │
         │     TransactionType (enum)           │
         │     Quantity                         │
         │     ReferenceType (string)           │
         │     ReferenceId (Guid?)              │
         │     TransactionDate                  │
         │     CreatedBy, CreatedAt             │
         └──────────────────────────────────────┘

┌──────────────────────────────────────┐    ┌──────────────────────────────────────┐
│          Customers                   │    │          Suppliers                   │
│ PK  Id                               │    │ PK  Id                               │
│ FK  TenantId                         │    │ FK  TenantId                         │
│     ShopName, OwnerName              │    │     Name, ContactPerson              │
│     Mobile, Email                    │    │     Mobile, Email                    │
│     Address, City, State, Pincode    │    │     Address, City, State, Pincode    │
│     GSTNumber                        │    │     GSTNumber                        │
│     CreditLimit, CreditDays          │    │     OpeningBalance                   │
│     OpeningBalance                   │    │     IsActive                         │
│     IsActive                         │    │     CreatedAt, UpdatedAt             │
│     CreatedAt, UpdatedAt             │    └────────┬─────────────────────────────┘
└────────┬─────────────────────────────┘             │
         │                                           │ 1:N
         │ 1:N                                       │
         │                                    ┌──────▼──────────────────────────────┐
         │                                    │         Purchases                   │
         │                                    │ PK  Id                              │
         │                                    │ FK  TenantId                        │
         │                                    │ FK  SupplierId                      │
         │                                    │     InvoiceNumber                   │
         │                                    │     PurchaseDate                    │
         │                                    │     SubTotal, Discount, Tax         │
         │                                    │     GrandTotal                      │
         │                                    │     Notes                           │
         │                                    │     CreatedBy, CreatedAt            │
         │                                    └────────┬────────────────────────────┘
         │                                             │
         │                                             │ 1:N
         │                                             │
         │                                    ┌────────▼────────────────────────────┐
         │                                    │      PurchaseItems                  │
         │                                    │ PK  Id                              │
         │                                    │ FK  PurchaseId                      │
         │                                    │ FK  ProductId                       │
         │                                    │     Quantity, UnitPrice             │
         │                                    │     Discount, Tax, Total            │
         │                                    └─────────────────────────────────────┘
         │
         │
  ┌──────▼──────────────────────────────┐
  │           Sales                     │
  │ PK  Id                              │
  │ FK  TenantId                        │
  │ FK  CustomerId                      │
  │     InvoiceNumber, SaleDate         │
  │     SubTotal, Discount, Tax         │
  │     GrandTotal                      │
  │     PaidAmount, DueAmount           │
  │     PaymentStatus (enum)            │
  │     Notes                           │
  │     CreatedBy, CreatedAt            │
  └──────┬──────────────────────────────┘
         │
         │ 1:N
         │
  ┌──────▼──────────────────────────────┐
  │         SaleItems                   │
  │ PK  Id                              │
  │ FK  SaleId                          │
  │ FK  ProductId                       │
  │     Quantity, UnitPrice             │
  │     Discount, Tax, Total            │
  └─────────────────────────────────────┘

┌──────────────────────────────────────┐
│          Payments                    │
│ PK  Id                               │
│ FK  TenantId                         │
│ FK  CustomerId                       │
│     PaymentDate, Amount              │
│     PaymentMethod (enum)             │
│     ReferenceNumber                  │
│     Notes                            │
│     CreatedBy, CreatedAt             │
└─────────────────────────────────────┘

┌──────────────────────────────────────┐
│     CustomerLedgerEntries            │
│ PK  Id                               │
│ FK  TenantId                         │
│ FK  CustomerId                       │
│     EntryDate, EntryType             │
│     ReferenceType, ReferenceId       │
│     Debit, Credit                    │
│     RunningBalance                   │
│     Notes                            │
│     CreatedAt                        │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│           Users                      │
│ PK  Id                               │
│ FK  TenantId                         │
│     Email, PasswordHash              │
│     FirstName, LastName              │
│     Mobile, IsActive                 │
│     Role (enum)                      │
│     CreatedAt, UpdatedAt             │
└──────────────────────────────────────┘
```

---

## 3. Table Definitions

### 3.1 Platform Tables

#### Tenants

```sql
CREATE TABLE Tenants (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code               NVARCHAR(50) NOT NULL UNIQUE,
    Name               NVARCHAR(200) NOT NULL,
    BusinessName       NVARCHAR(200) NOT NULL,
    Email              NVARCHAR(200) NOT NULL,
    Phone              NVARCHAR(20),
    Address            NVARCHAR(500),
    City               NVARCHAR(100),
    State              NVARCHAR(100),
    Pincode            NVARCHAR(10),
    GSTNumber          NVARCHAR(20),
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,

    INDEX IX_Tenants_Code (Code),
    INDEX IX_Tenants_IsActive (IsActive)
);
```

**PostgreSQL Version:**
```sql
CREATE TABLE Tenants (
    Id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Code               VARCHAR(50) NOT NULL UNIQUE,
    Name               VARCHAR(200) NOT NULL,
    BusinessName       VARCHAR(200) NOT NULL,
    Email              VARCHAR(200) NOT NULL,
    Phone              VARCHAR(20),
    Address            VARCHAR(500),
    City               VARCHAR(100),
    State              VARCHAR(100),
    Pincode            VARCHAR(10),
    GSTNumber          VARCHAR(20),
    IsActive           BOOLEAN NOT NULL DEFAULT true,
    CreatedAt          TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt          TIMESTAMP
);

CREATE INDEX IX_Tenants_Code ON Tenants(Code);
CREATE INDEX IX_Tenants_IsActive ON Tenants(IsActive);
```

---

### 3.2 Identity Tables

#### Users

```sql
CREATE TABLE Users (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    Email              NVARCHAR(200) NOT NULL,
    PasswordHash       NVARCHAR(500) NOT NULL,
    FirstName          NVARCHAR(100) NOT NULL,
    LastName           NVARCHAR(100) NOT NULL,
    Mobile             NVARCHAR(20),
    Role               NVARCHAR(50) NOT NULL,  -- Admin, Manager, SalesStaff, Viewer
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,
    CreatedBy          UNIQUEIDENTIFIER,
    UpdatedBy          UNIQUEIDENTIFIER,

    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_Users_TenantId_Email UNIQUE (TenantId, Email),

    INDEX IX_Users_TenantId (TenantId),
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_Role (Role),
    INDEX IX_Users_IsActive (IsActive)
);
```

---

### 3.3 Master Data Tables

#### Categories

```sql
CREATE TABLE Categories (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    Name               NVARCHAR(200) NOT NULL,
    Description        NVARCHAR(500),
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,
    CreatedBy          UNIQUEIDENTIFIER,
    UpdatedBy          UNIQUEIDENTIFIER,

    CONSTRAINT FK_Categories_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_Categories_TenantId_Name UNIQUE (TenantId, Name),

    INDEX IX_Categories_TenantId (TenantId),
    INDEX IX_Categories_IsActive (TenantId, IsActive)
);
```

#### Brands

```sql
CREATE TABLE Brands (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    Name               NVARCHAR(200) NOT NULL,
    Description        NVARCHAR(500),
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,
    CreatedBy          UNIQUEIDENTIFIER,
    UpdatedBy          UNIQUEIDENTIFIER,

    CONSTRAINT FK_Brands_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_Brands_TenantId_Name UNIQUE (TenantId, Name),

    INDEX IX_Brands_TenantId (TenantId),
    INDEX IX_Brands_IsActive (TenantId, IsActive)
);
```

#### Units

```sql
CREATE TABLE Units (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    Name               NVARCHAR(50) NOT NULL,      -- Kg, Pcs, Ltr, Box, etc.
    ShortName          NVARCHAR(10) NOT NULL,       -- kg, pcs, l, box
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,

    CONSTRAINT FK_Units_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_Units_TenantId_Name UNIQUE (TenantId, Name),

    INDEX IX_Units_TenantId (TenantId)
);
```

#### Products

```sql
CREATE TABLE Products (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    CategoryId         UNIQUEIDENTIFIER,
    BrandId            UNIQUEIDENTIFIER,
    UnitId             UNIQUEIDENTIFIER NOT NULL,
    Name               NVARCHAR(200) NOT NULL,
    SKU                NVARCHAR(100) NOT NULL,
    Barcode            NVARCHAR(100),
    Description        NVARCHAR(500),
    PurchasePrice      DECIMAL(18, 2) NOT NULL DEFAULT 0,
    SellingPrice       DECIMAL(18, 2) NOT NULL DEFAULT 0,
    MinimumStock       INT NOT NULL DEFAULT 0,
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,
    CreatedBy          UNIQUEIDENTIFIER,
    UpdatedBy          UNIQUEIDENTIFIER,

    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES Brands(Id),
    CONSTRAINT FK_Products_Units FOREIGN KEY (UnitId) REFERENCES Units(Id),
    CONSTRAINT UQ_Products_TenantId_SKU UNIQUE (TenantId, SKU),

    INDEX IX_Products_TenantId (TenantId),
    INDEX IX_Products_TenantId_IsActive (TenantId, IsActive),
    INDEX IX_Products_TenantId_Name (TenantId, Name),
    INDEX IX_Products_TenantId_SKU (TenantId, SKU),
    INDEX IX_Products_TenantId_Barcode (TenantId, Barcode),
    INDEX IX_Products_CategoryId (CategoryId),
    INDEX IX_Products_BrandId (BrandId)
);
```

#### Customers

```sql
CREATE TABLE Customers (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    ShopName           NVARCHAR(200) NOT NULL,
    OwnerName          NVARCHAR(200) NOT NULL,
    Mobile             NVARCHAR(20) NOT NULL,
    Email              NVARCHAR(200),
    Address            NVARCHAR(500),
    City               NVARCHAR(100),
    State              NVARCHAR(100),
    Pincode            NVARCHAR(10),
    GSTNumber          NVARCHAR(20),
    CreditLimit        DECIMAL(18, 2) NOT NULL DEFAULT 0,
    CreditDays         INT NOT NULL DEFAULT 0,
    OpeningBalance     DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,
    CreatedBy          UNIQUEIDENTIFIER,
    UpdatedBy          UNIQUEIDENTIFIER,

    CONSTRAINT FK_Customers_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),

    INDEX IX_Customers_TenantId (TenantId),
    INDEX IX_Customers_TenantId_IsActive (TenantId, IsActive),
    INDEX IX_Customers_TenantId_ShopName (TenantId, ShopName),
    INDEX IX_Customers_TenantId_Mobile (TenantId, Mobile)
);
```

#### Suppliers

```sql
CREATE TABLE Suppliers (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    Name               NVARCHAR(200) NOT NULL,
    ContactPerson      NVARCHAR(200),
    Mobile             NVARCHAR(20) NOT NULL,
    Email              NVARCHAR(200),
    Address            NVARCHAR(500),
    City               NVARCHAR(100),
    State              NVARCHAR(100),
    Pincode            NVARCHAR(10),
    GSTNumber          NVARCHAR(20),
    OpeningBalance     DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive           BIT NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2,
    CreatedBy          UNIQUEIDENTIFIER,
    UpdatedBy          UNIQUEIDENTIFIER,

    CONSTRAINT FK_Suppliers_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),

    INDEX IX_Suppliers_TenantId (TenantId),
    INDEX IX_Suppliers_TenantId_IsActive (TenantId, IsActive),
    INDEX IX_Suppliers_TenantId_Name (TenantId, Name)
);
```

---

### 3.4 Inventory Tables

#### StockBalances

```sql
CREATE TABLE StockBalances (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    ProductId          UNIQUEIDENTIFIER NOT NULL,
    QuantityOnHand     INT NOT NULL DEFAULT 0,
    ReservedQuantity   INT NOT NULL DEFAULT 0,
    UpdatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_StockBalances_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_StockBalances_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_StockBalances_TenantId_ProductId UNIQUE (TenantId, ProductId),
    CONSTRAINT CK_StockBalances_QuantityOnHand CHECK (QuantityOnHand >= 0),

    INDEX IX_StockBalances_TenantId (TenantId),
    INDEX IX_StockBalances_ProductId (ProductId),
    INDEX IX_StockBalances_TenantId_QuantityOnHand (TenantId, QuantityOnHand)
);
```

#### InventoryTransactions

```sql
CREATE TABLE InventoryTransactions (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    ProductId          UNIQUEIDENTIFIER NOT NULL,
    TransactionType    NVARCHAR(50) NOT NULL,  -- OpeningStock, Purchase, Sale, Adjustment
    Quantity           INT NOT NULL,            -- Positive for In, Negative for Out
    ReferenceType      NVARCHAR(50),           -- Purchase, Sale, Adjustment, etc.
    ReferenceId        UNIQUEIDENTIFIER,
    TransactionDate    DATETIME2 NOT NULL,
    Notes              NVARCHAR(500),
    CreatedBy          UNIQUEIDENTIFIER NOT NULL,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_InventoryTransactions_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_InventoryTransactions_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),

    INDEX IX_InventoryTransactions_TenantId (TenantId),
    INDEX IX_InventoryTransactions_ProductId (ProductId),
    INDEX IX_InventoryTransactions_TenantId_TransactionDate (TenantId, TransactionDate),
    INDEX IX_InventoryTransactions_ReferenceType_ReferenceId (ReferenceType, ReferenceId)
);
```

---

### 3.5 Transaction Tables

#### Purchases

```sql
CREATE TABLE Purchases (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    SupplierId         UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber      NVARCHAR(100) NOT NULL,
    PurchaseDate       DATE NOT NULL,
    SubTotal           DECIMAL(18, 2) NOT NULL,
    Discount           DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Tax                DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GrandTotal         DECIMAL(18, 2) NOT NULL,
    Notes              NVARCHAR(500),
    CreatedBy          UNIQUEIDENTIFIER NOT NULL,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Purchases_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Purchases_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    CONSTRAINT UQ_Purchases_TenantId_InvoiceNumber UNIQUE (TenantId, InvoiceNumber),

    INDEX IX_Purchases_TenantId (TenantId),
    INDEX IX_Purchases_SupplierId (SupplierId),
    INDEX IX_Purchases_TenantId_PurchaseDate (TenantId, PurchaseDate),
    INDEX IX_Purchases_TenantId_InvoiceNumber (TenantId, InvoiceNumber)
);
```

#### PurchaseItems

```sql
CREATE TABLE PurchaseItems (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PurchaseId         UNIQUEIDENTIFIER NOT NULL,
    ProductId          UNIQUEIDENTIFIER NOT NULL,
    Quantity           INT NOT NULL,
    UnitPrice          DECIMAL(18, 2) NOT NULL,
    Discount           DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Tax                DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Total              DECIMAL(18, 2) NOT NULL,

    CONSTRAINT FK_PurchaseItems_Purchases FOREIGN KEY (PurchaseId) REFERENCES Purchases(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_PurchaseItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_PurchaseItems_UnitPrice CHECK (UnitPrice >= 0),

    INDEX IX_PurchaseItems_PurchaseId (PurchaseId),
    INDEX IX_PurchaseItems_ProductId (ProductId)
);
```

#### Sales

```sql
CREATE TABLE Sales (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    CustomerId         UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber      NVARCHAR(100) NOT NULL,
    SaleDate           DATE NOT NULL,
    SubTotal           DECIMAL(18, 2) NOT NULL,
    Discount           DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Tax                DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GrandTotal         DECIMAL(18, 2) NOT NULL,
    PaidAmount         DECIMAL(18, 2) NOT NULL DEFAULT 0,
    DueAmount          DECIMAL(18, 2) NOT NULL DEFAULT 0,
    PaymentStatus      NVARCHAR(20) NOT NULL,  -- Unpaid, Partial, Paid
    Notes              NVARCHAR(500),
    CreatedBy          UNIQUEIDENTIFIER NOT NULL,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Sales_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Sales_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT UQ_Sales_TenantId_InvoiceNumber UNIQUE (TenantId, InvoiceNumber),
    CONSTRAINT CK_Sales_PaidAmount CHECK (PaidAmount >= 0 AND PaidAmount <= GrandTotal),
    CONSTRAINT CK_Sales_DueAmount CHECK (DueAmount >= 0),

    INDEX IX_Sales_TenantId (TenantId),
    INDEX IX_Sales_CustomerId (CustomerId),
    INDEX IX_Sales_TenantId_SaleDate (TenantId, SaleDate),
    INDEX IX_Sales_TenantId_PaymentStatus (TenantId, PaymentStatus),
    INDEX IX_Sales_TenantId_InvoiceNumber (TenantId, InvoiceNumber)
);
```

#### SaleItems

```sql
CREATE TABLE SaleItems (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SaleId             UNIQUEIDENTIFIER NOT NULL,
    ProductId          UNIQUEIDENTIFIER NOT NULL,
    Quantity           INT NOT NULL,
    UnitPrice          DECIMAL(18, 2) NOT NULL,
    Discount           DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Tax                DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Total              DECIMAL(18, 2) NOT NULL,

    CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SaleItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_SaleItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SaleItems_UnitPrice CHECK (UnitPrice >= 0),

    INDEX IX_SaleItems_SaleId (SaleId),
    INDEX IX_SaleItems_ProductId (ProductId)
);
```

---

### 3.6 Payment & Ledger Tables

#### Payments

```sql
CREATE TABLE Payments (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    CustomerId         UNIQUEIDENTIFIER NOT NULL,
    PaymentDate        DATE NOT NULL,
    Amount             DECIMAL(18, 2) NOT NULL,
    PaymentMethod      NVARCHAR(50) NOT NULL,  -- Cash, Cheque, BankTransfer, UPI, Card
    ReferenceNumber    NVARCHAR(100),
    Notes              NVARCHAR(500),
    CreatedBy          UNIQUEIDENTIFIER NOT NULL,
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Payments_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Payments_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),

    INDEX IX_Payments_TenantId (TenantId),
    INDEX IX_Payments_CustomerId (CustomerId),
    INDEX IX_Payments_TenantId_PaymentDate (TenantId, PaymentDate)
);
```

#### CustomerLedgerEntries

```sql
CREATE TABLE CustomerLedgerEntries (
    Id                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    CustomerId         UNIQUEIDENTIFIER NOT NULL,
    EntryDate          DATE NOT NULL,
    EntryType          NVARCHAR(50) NOT NULL,  -- OpeningBalance, Sale, Payment, Adjustment
    ReferenceType      NVARCHAR(50),           -- Sale, Payment, etc.
    ReferenceId        UNIQUEIDENTIFIER,
    Debit              DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Credit             DECIMAL(18, 2) NOT NULL DEFAULT 0,
    RunningBalance     DECIMAL(18, 2) NOT NULL,
    Notes              NVARCHAR(500),
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_CustomerLedgerEntries_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_CustomerLedgerEntries_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT CK_CustomerLedgerEntries_DebitOrCredit CHECK (
        (Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0)
    ),

    INDEX IX_CustomerLedgerEntries_TenantId (TenantId),
    INDEX IX_CustomerLedgerEntries_CustomerId (CustomerId),
    INDEX IX_CustomerLedgerEntries_TenantId_CustomerId_EntryDate (TenantId, CustomerId, EntryDate),
    INDEX IX_CustomerLedgerEntries_ReferenceType_ReferenceId (ReferenceType, ReferenceId)
);
```

---

## 4. Indexes Strategy

### 4.1 Primary Indexes (Already Defined Above)

All primary keys are clustered indexes by default.

### 4.2 Tenant Isolation Indexes

**Critical:** Every tenant-owned table must have an index on `TenantId`:

```sql
-- All tenant-owned tables
CREATE INDEX IX_{TableName}_TenantId ON {TableName}(TenantId);
```

### 4.3 Foreign Key Indexes

Foreign key columns automatically indexed for join performance:

```sql
-- Products
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_BrandId ON Products(BrandId);
CREATE INDEX IX_Products_UnitId ON Products(UnitId);

-- Purchases & Sales
CREATE INDEX IX_Purchases_SupplierId ON Purchases(SupplierId);
CREATE INDEX IX_Sales_CustomerId ON Sales(CustomerId);

-- Line Items
CREATE INDEX IX_PurchaseItems_PurchaseId ON PurchaseItems(PurchaseId);
CREATE INDEX IX_PurchaseItems_ProductId ON PurchaseItems(ProductId);
CREATE INDEX IX_SaleItems_SaleId ON SaleItems(SaleId);
CREATE INDEX IX_SaleItems_ProductId ON SaleItems(ProductId);
```

### 4.4 Search & Lookup Indexes

For common search operations:

```sql
-- Product search
CREATE INDEX IX_Products_TenantId_Name ON Products(TenantId, Name);
CREATE INDEX IX_Products_TenantId_SKU ON Products(TenantId, SKU);
CREATE INDEX IX_Products_TenantId_Barcode ON Products(TenantId, Barcode) WHERE Barcode IS NOT NULL;

-- Customer search
CREATE INDEX IX_Customers_TenantId_ShopName ON Customers(TenantId, ShopName);
CREATE INDEX IX_Customers_TenantId_Mobile ON Customers(TenantId, Mobile);

-- Supplier search
CREATE INDEX IX_Suppliers_TenantId_Name ON Suppliers(TenantId, Name);
```

### 4.5 Reporting Indexes

For common report queries:

```sql
-- Sales reports
CREATE INDEX IX_Sales_TenantId_SaleDate ON Sales(TenantId, SaleDate);
CREATE INDEX IX_Sales_TenantId_PaymentStatus ON Sales(TenantId, PaymentStatus);

-- Purchase reports
CREATE INDEX IX_Purchases_TenantId_PurchaseDate ON Purchases(TenantId, PurchaseDate);

-- Inventory reports
CREATE INDEX IX_InventoryTransactions_TenantId_TransactionDate
    ON InventoryTransactions(TenantId, TransactionDate);

-- Low stock report
CREATE INDEX IX_StockBalances_TenantId_QuantityOnHand
    ON StockBalances(TenantId, QuantityOnHand);

-- Customer outstanding
CREATE INDEX IX_CustomerLedgerEntries_TenantId_CustomerId_EntryDate
    ON CustomerLedgerEntries(TenantId, CustomerId, EntryDate);
```

### 4.6 Covering Indexes (Future Optimization)

For frequently accessed queries, consider covering indexes:

```sql
-- Example: Product list with category and brand
CREATE INDEX IX_Products_List_Covering
    ON Products(TenantId, IsActive)
    INCLUDE (Name, SKU, CategoryId, BrandId, SellingPrice);
```

---

## 5. Database Constraints

### 5.1 Primary Key Constraints

All tables have GUID primary keys:
- Distributed-friendly (no sequence conflicts)
- Non-sequential (security benefit)
- Compatible with both SQL Server and PostgreSQL

### 5.2 Foreign Key Constraints

All relationships enforced at database level:
- Referential integrity guaranteed
- Cascade delete where appropriate (line items)
- Restrict delete for master data

### 5.3 Unique Constraints

#### Tenant-Scoped Uniqueness
```sql
-- Products: SKU unique per tenant
CONSTRAINT UQ_Products_TenantId_SKU UNIQUE (TenantId, SKU);

-- Sales: Invoice number unique per tenant
CONSTRAINT UQ_Sales_TenantId_InvoiceNumber UNIQUE (TenantId, InvoiceNumber);

-- Purchases: Invoice number unique per tenant
CONSTRAINT UQ_Purchases_TenantId_InvoiceNumber UNIQUE (TenantId, InvoiceNumber);

-- Categories: Name unique per tenant
CONSTRAINT UQ_Categories_TenantId_Name UNIQUE (TenantId, Name);

-- Brands: Name unique per tenant
CONSTRAINT UQ_Brands_TenantId_Name UNIQUE (TenantId, Name);
```

#### Global Uniqueness
```sql
-- Tenant: Code globally unique
CONSTRAINT UQ_Tenants_Code UNIQUE (Code);

-- Users: Email unique per tenant
CONSTRAINT UQ_Users_TenantId_Email UNIQUE (TenantId, Email);
```

### 5.4 Check Constraints

Business rule enforcement:

```sql
-- Stock cannot be negative
CONSTRAINT CK_StockBalances_QuantityOnHand CHECK (QuantityOnHand >= 0);

-- Quantity must be positive
CONSTRAINT CK_SaleItems_Quantity CHECK (Quantity > 0);
CONSTRAINT CK_PurchaseItems_Quantity CHECK (Quantity > 0);

-- Price must be non-negative
CONSTRAINT CK_SaleItems_UnitPrice CHECK (UnitPrice >= 0);
CONSTRAINT CK_PurchaseItems_UnitPrice CHECK (UnitPrice >= 0);

-- Payment amount must be positive and within grand total
CONSTRAINT CK_Sales_PaidAmount CHECK (PaidAmount >= 0 AND PaidAmount <= GrandTotal);
CONSTRAINT CK_Payments_Amount CHECK (Amount > 0);

-- Ledger: Either debit or credit, not both
CONSTRAINT CK_CustomerLedgerEntries_DebitOrCredit CHECK (
    (Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0)
);
```

---

## 6. Migration Strategy

### 6.1 EF Core Migrations

**Initial Migration:**
```bash
# SQL Server
dotnet ef migrations add InitialCreate --context ApplicationDbContext

# PostgreSQL (if separate migrations)
dotnet ef migrations add InitialCreate --context ApplicationDbContext --provider Npgsql
```

**Apply Migrations:**
```bash
# Development (SQL Server)
dotnet ef database update

# Production (PostgreSQL)
dotnet ef database update --connection "Host=prod-db;Database=inventoryproc;..."
```

### 6.2 Migration Scripts

**Idempotent Scripts:**
```sql
-- Check if table exists before creating
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE Tenants (...);
END
```

### 6.3 Data Migration Scripts

**Migrating from Excel to Database:**
```sql
-- After initial schema creation, run data import procedures
EXEC ImportProductsFromExcel @TenantId, @FilePath;
EXEC ImportCustomersFromExcel @TenantId, @FilePath;
EXEC ImportOpeningStock @TenantId, @FilePath;
```

---

## 7. Data Seeding

### 7.1 Initial Seed Data

**Default Units:**
```sql
INSERT INTO Units (Id, TenantId, Name, ShortName, IsActive)
VALUES
    (NEWID(), @TenantId, 'Pieces', 'Pcs', 1),
    (NEWID(), @TenantId, 'Kilogram', 'Kg', 1),
    (NEWID(), @TenantId, 'Liter', 'Ltr', 1),
    (NEWID(), @TenantId, 'Box', 'Box', 1),
    (NEWID(), @TenantId, 'Dozen', 'Dzn', 1);
```

**Default Categories (Optional):**
```sql
INSERT INTO Categories (Id, TenantId, Name, IsActive)
VALUES
    (NEWID(), @TenantId, 'General', 1),
    (NEWID(), @TenantId, 'Electronics', 1),
    (NEWID(), @TenantId, 'Food & Beverage', 1),
    (NEWID(), @TenantId, 'Household', 1);
```

### 7.2 Demo Data

For demo tenant, create:
- 20-50 products across categories
- 10-20 customers
- 3-5 suppliers
- Opening stock for products
- 20-30 historical purchases
- 50-100 historical sales
- 30-50 payments
- Corresponding inventory transactions and ledger entries

---

## Appendix A: Database Sizing Estimates

### Small Tenant (100 customers, 500 products, 1000 sales/month)

| Table | Rows/Month | Rows/Year | Estimated Size/Year |
|-------|-----------|-----------|---------------------|
| Products | - | 500 | 100 KB |
| Customers | - | 100 | 50 KB |
| Sales | 1,000 | 12,000 | 5 MB |
| SaleItems | 5,000 | 60,000 | 20 MB |
| Purchases | 200 | 2,400 | 1 MB |
| InventoryTransactions | 6,000 | 72,000 | 25 MB |
| CustomerLedgerEntries | 2,000 | 24,000 | 10 MB |
| **Total** | - | - | **~60 MB/year** |

### Large Tenant (10,000 customers, 100,000 products, 50,000 sales/month)

| Table | Rows/Month | Rows/Year | Estimated Size/Year |
|-------|-----------|-----------|---------------------|
| Products | - | 100,000 | 20 MB |
| Customers | - | 10,000 | 5 MB |
| Sales | 50,000 | 600,000 | 250 MB |
| SaleItems | 250,000 | 3,000,000 | 1 GB |
| Purchases | 10,000 | 120,000 | 50 MB |
| InventoryTransactions | 300,000 | 3,600,000 | 1.2 GB |
| CustomerLedgerEntries | 100,000 | 1,200,000 | 500 MB |
| **Total** | - | - | **~3 GB/year** |

---

## Appendix B: Database Portability Notes

### SQL Server vs PostgreSQL Differences

| Feature | SQL Server | PostgreSQL |
|---------|-----------|------------|
| **GUID Generation** | `NEWID()` | `gen_random_uuid()` |
| **String Type** | `NVARCHAR` | `VARCHAR` |
| **Boolean Type** | `BIT` | `BOOLEAN` |
| **Date Type** | `DATETIME2` | `TIMESTAMP` |
| **Current Time** | `GETUTCDATE()` | `NOW()` |
| **Auto-increment** | `IDENTITY` | `SERIAL` / `IDENTITY` (PG 10+) |
| **Schema** | `dbo` default | `public` default |

### EF Core Configuration for Portability

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Use HasDefaultValueSql with provider-specific SQL
    modelBuilder.Entity<Tenant>()
        .Property(t => t.CreatedAt)
        .HasDefaultValueSql("GETUTCDATE()");  // SQL Server

    // Or configure in migration based on provider
    if (Database.IsSqlServer())
    {
        // SQL Server specific
    }
    else if (Database.IsNpgsql())
    {
        // PostgreSQL specific
    }
}
```

---

**Document Status:** This is a living document and will be updated as database schema evolves during implementation.
