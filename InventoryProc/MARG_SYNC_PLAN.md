# Marg ERP Sync & Analytics Integration Plan
**Date:** September 24, 2026
**Version:** 1.0
**Status:** Design Phase

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Sync Architecture](#sync-architecture)
3. [Marg Database Schema](#marg-database-schema)
4. [Data Export Utility](#data-export-utility)
5. [Sync Service Design](#sync-service-design)
6. [Analytics & Reports](#analytics--reports)
7. [Implementation Phases](#implementation-phases)

---

## 1. Overview

### Objective
Create a **one-way sync** from Marg ERP → InventoryProc for advanced analytics while shops continue using Marg for daily operations.

### Use Cases
- ✅ Multi-shop owners view consolidated data
- ✅ Advanced analytics not available in Marg
- ✅ Cloud-based reporting accessible from anywhere
- ✅ Historical trend analysis
- ✅ Expiry tracking and alerts
- ✅ Doctor/prescription analytics (medical shops)

### Sync Approach
```
┌─────────────┐         ┌──────────────┐         ┌─────────────────┐
│  Marg ERP   │ ──────> │ Sync Utility │ ──────> │  InventoryProc  │
│  (Local DB) │         │   (Windows)  │         │  (Cloud/Server) │
└─────────────┘         └──────────────┘         └─────────────────┘
     SQL Server              .NET App                  PostgreSQL
                                │
                                ├─ Read Marg DB
                                ├─ Transform Data
                                ├─ Upload via API
                                └─ Schedule (Daily/Hourly)
```

---

## 2. Sync Architecture

### 2.1 Architecture Overview

```
┌───────────────────────────────────────────────────────────────┐
│                         SHOP (Local)                          │
│                                                                │
│  ┌────────────┐           ┌─────────────────────────────┐   │
│  │  Marg ERP  │           │    Sync Utility (Service)    │   │
│  │            │           │                              │   │
│  │ SQL Server │◄─────────┤  - Windows Service           │   │
│  │  Database  │  Read     │  - Runs every X hours        │   │
│  └────────────┘           │  - Transform & Upload        │   │
│                           │  - Error handling            │   │
│                           └──────────────┬───────────────┘   │
│                                          │ HTTPS/API          │
└──────────────────────────────────────────┼───────────────────┘
                                           │
                                           ▼
                         ┌─────────────────────────────────┐
                         │   CLOUD SERVER (InventoryProc)  │
                         │                                 │
                         │  ┌──────────────────────────┐  │
                         │  │   Sync API Endpoint      │  │
                         │  │   /api/marg-sync         │  │
                         │  └───────────┬──────────────┘  │
                         │              │                  │
                         │  ┌───────────▼──────────────┐  │
                         │  │  Data Validation &       │  │
                         │  │  Transformation          │  │
                         │  └───────────┬──────────────┘  │
                         │              │                  │
                         │  ┌───────────▼──────────────┐  │
                         │  │   PostgreSQL Database    │  │
                         │  │   (InventoryProc)        │  │
                         │  └──────────────────────────┘  │
                         │              │                  │
                         │  ┌───────────▼──────────────┐  │
                         │  │  Analytics Engine        │  │
                         │  │  Reports & Dashboards    │  │
                         │  └──────────────────────────┘  │
                         └─────────────────────────────────┘
```

### 2.2 Components

#### **Component 1: Marg Sync Utility (Desktop Application)**

**Technology:** .NET 9 Windows Service
**Purpose:** Extract data from Marg SQL Server and upload to cloud

**Features:**
- Windows Service (runs in background)
- Configurable sync schedule
- Incremental sync (only changed data)
- Error logging and retry mechanism
- Status dashboard (system tray app)
- Pause/Resume capability

#### **Component 2: Sync API (Cloud Server)**

**Technology:** ASP.NET Core Web API
**Purpose:** Receive and process Marg data

**Endpoints:**
```
POST /api/marg-sync/authenticate
POST /api/marg-sync/products
POST /api/marg-sync/purchases
POST /api/marg-sync/sales
POST /api/marg-sync/customers
POST /api/marg-sync/suppliers
POST /api/marg-sync/stock
GET  /api/marg-sync/status
```

#### **Component 3: Analytics Engine**

**Purpose:** Process synced data for insights

**Features:**
- Sales trends and forecasting
- Expiry management
- Stock optimization
- Profit analysis
- Doctor/prescription tracking (medical)
- Supplier performance

---

## 3. Marg Database Schema

### 3.1 Key Marg Tables (Medical Shop Focus)

#### **ItemMaster** (Products/Medicines)
```sql
-- Marg Table Structure
ItemMaster
├── ItemCode          (VARCHAR)    -- Unique product code
├── ItemName          (VARCHAR)    -- Product name
├── CompanyName       (VARCHAR)    -- Manufacturer
├── Packing           (VARCHAR)    -- Pack size (10x10, etc.)
├── Composition       (VARCHAR)    -- Salt composition
├── HSNCode           (VARCHAR)    -- GST HSN code
├── MRP               (DECIMAL)    -- Maximum Retail Price
├── PurchaseRate      (DECIMAL)    -- Last purchase rate
├── SaleRate          (DECIMAL)    -- Selling price
├── MinStock          (DECIMAL)    -- Minimum stock level
├── MaxStock          (DECIMAL)    -- Maximum stock level
├── ScheduleType      (VARCHAR)    -- H, H1, X, etc.
├── IsActive          (BIT)        -- Active status
└── GroupName         (VARCHAR)    -- Category/Group

-- Map to InventoryProc.Product
ItemCode      → Code
ItemName      → Name
CompanyName   → Brand (create if not exists)
Packing       → Description
Composition   → Description (append)
HSNCode       → HSNCode
MRP           → MRP
PurchaseRate  → PurchasePrice
SaleRate      → SalePrice
MinStock      → MinStockLevel
MaxStock      → MaxStockLevel
GroupName     → Category (create if not exists)
```

#### **BatchMaster** (Batch/Expiry Tracking)
```sql
BatchMaster
├── ItemCode          (VARCHAR)    -- Links to ItemMaster
├── BatchNo           (VARCHAR)    -- Batch number
├── MfgDate           (DATE)       -- Manufacturing date
├── ExpDate           (DATE)       -- Expiry date
├── Quantity          (DECIMAL)    -- Quantity in batch
├── PurchaseRate      (DECIMAL)    -- Purchase rate for this batch
├── MRP               (DECIMAL)    -- MRP for this batch
└── SupplierCode      (VARCHAR)    -- Supplier reference

-- Map to new table: ProductBatch
-- (Need to add this table to InventoryProc)
```

#### **SaleMaster** (Sales/Invoices)
```sql
SaleMaster
├── InvNo             (VARCHAR)    -- Invoice number
├── InvDate           (DATE)       -- Invoice date
├── PartyCode         (VARCHAR)    -- Customer code
├── PartyName         (VARCHAR)    -- Customer name
├── GrossAmount       (DECIMAL)    -- Total before tax
├── TaxAmount         (DECIMAL)    -- GST amount
├── DiscountAmount    (DECIMAL)    -- Discount given
├── NetAmount         (DECIMAL)    -- Final amount
├── PaidAmount        (DECIMAL)    -- Amount paid
├── DoctorName        (VARCHAR)    -- Prescribing doctor (medical)
└── PaymentMode       (VARCHAR)    -- Cash/Card/UPI

-- Map to InventoryProc.Invoice/SalesOrder
```

#### **SaleDetail** (Sale Items)
```sql
SaleDetail
├── InvNo             (VARCHAR)    -- Links to SaleMaster
├── ItemCode          (VARCHAR)    -- Product code
├── ItemName          (VARCHAR)    -- Product name
├── BatchNo           (VARCHAR)    -- Batch number
├── Quantity          (DECIMAL)    -- Quantity sold
├── Rate              (DECIMAL)    -- Sale rate
├── Discount          (DECIMAL)    -- Item discount
├── TaxPer            (DECIMAL)    -- Tax percentage
├── TaxAmount         (DECIMAL)    -- Tax amount
└── Amount            (DECIMAL)    -- Line total

-- Map to InventoryProc.InvoiceItem
```

#### **PurchaseMaster** & **PurchaseDetail**
```sql
-- Similar structure to Sales tables
-- Map to InventoryProc.PurchaseOrder and items
```

#### **PartyMaster** (Customers & Suppliers)
```sql
PartyMaster
├── PartyCode         (VARCHAR)
├── PartyName         (VARCHAR)
├── PartyType         (VARCHAR)    -- Customer/Supplier
├── Address           (VARCHAR)
├── City              (VARCHAR)
├── State             (VARCHAR)
├── Pincode           (VARCHAR)
├── Phone             (VARCHAR)
├── Mobile            (VARCHAR)
├── Email             (VARCHAR)
├── GSTNo             (VARCHAR)
├── PANNo             (VARCHAR)
├── CreditLimit       (DECIMAL)
├── CreditDays        (INT)
└── OpeningBalance    (DECIMAL)

-- Map based on PartyType:
-- 'Customer' → InventoryProc.Customer
-- 'Supplier' → InventoryProc.Vendor
```

#### **StockSummary** (Current Stock)
```sql
StockSummary
├── ItemCode          (VARCHAR)
├── BatchNo           (VARCHAR)
├── CurrentStock      (DECIMAL)
├── LastPurchaseRate  (DECIMAL)
└── LastPurchaseDate  (DATE)

-- Use to update InventoryProc.Product.CurrentStock
```

### 3.2 Marg Connection String

```csharp
// Typical Marg SQL Server connection
Server=localhost\\SQLEXPRESS;Database=MARGDATA;User Id=sa;Password=marg123;
```

**Common Marg Installations:**
- SQL Server Express (most common)
- MS Access (older versions)
- Default Database Name: `MARGDATA`, `MargPharma`, `MargRetail`

---

## 4. Data Export Utility

### 4.1 Utility Design

```
┌─────────────────────────────────────────────────────────────┐
│             Marg Sync Utility (Windows App)                 │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │              Configuration Tab                      │    │
│  │  ┌──────────────────────────────────────────────┐  │    │
│  │  │  Marg Database Connection:                   │  │    │
│  │  │  Server: [localhost\SQLEXPRESS       ] [Test]│  │    │
│  │  │  Database: [MARGDATA                ]        │  │    │
│  │  │  Username: [sa                      ]        │  │    │
│  │  │  Password: [••••••                  ]        │  │    │
│  │  │                                               │  │    │
│  │  │  Cloud API Connection:                       │  │    │
│  │  │  Server URL: [https://api.inventory.com]     │  │    │
│  │  │  API Key: [•••••••••••••••••••••••]   [Test]│  │    │
│  │  │  Shop Code: [SHOP001               ]        │  │    │
│  │  └──────────────────────────────────────────────┘  │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │              Sync Schedule Tab                      │    │
│  │  ┌──────────────────────────────────────────────┐  │    │
│  │  │  ☑ Auto Sync Enabled                         │  │    │
│  │  │  Frequency: [Every 6 hours          ▼]       │  │    │
│  │  │  Next Sync: 24-Sep-2026 09:00 PM             │  │    │
│  │  │                                               │  │    │
│  │  │  Sync Options:                               │  │    │
│  │  │  ☑ Product Master                            │  │    │
│  │  │  ☑ Batch/Expiry Data                         │  │    │
│  │  │  ☑ Sales Data (Last 30 days)                 │  │    │
│  │  │  ☑ Purchase Data (Last 30 days)              │  │    │
│  │  │  ☑ Customer Master                           │  │    │
│  │  │  ☑ Supplier Master                           │  │    │
│  │  │  ☑ Current Stock                             │  │    │
│  │  │                                               │  │    │
│  │  │  [ Sync Now ]  [ Stop ]                      │  │    │
│  │  └──────────────────────────────────────────────┘  │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │              Status & Logs Tab                      │    │
│  │  ┌──────────────────────────────────────────────┐  │    │
│  │  │  Last Sync: 24-Sep-2026 03:00 PM   ✅        │  │    │
│  │  │  Status: Completed Successfully               │  │    │
│  │  │                                               │  │    │
│  │  │  Records Synced:                             │  │    │
│  │  │  - Products: 1,234 items                     │  │    │
│  │  │  - Sales: 456 invoices (last 30 days)       │  │    │
│  │  │  - Purchases: 89 orders                      │  │    │
│  │  │  - Customers: 567 records                    │  │    │
│  │  │                                               │  │    │
│  │  │  Recent Activity:                            │  │    │
│  │  │  [Log Text Area - scrollable]                │  │    │
│  │  │  15:00 - Started sync...                     │  │    │
│  │  │  15:01 - Synced products (1234 items)        │  │    │
│  │  │  15:02 - Synced sales (456 invoices)         │  │    │
│  │  │  15:03 - Completed successfully              │  │    │
│  │  │                                               │  │    │
│  │  │  [ View Full Log ]  [ Clear Log ]            │  │    │
│  │  └──────────────────────────────────────────────┘  │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  [ Start Service ]  [ Stop Service ]  [ Exit ]             │
└─────────────────────────────────────────────────────────────┘

System Tray Icon: [📊] (Green = Running, Red = Stopped, Yellow = Syncing)
```

### 4.2 Utility Features

#### **Feature 1: Incremental Sync**
```csharp
// Track last sync timestamp for each entity
LastSync:
  Products: 2026-09-24 15:00:00
  Sales: 2026-09-24 15:00:00
  Purchases: 2026-09-24 15:00:00

// Only sync changed records
SELECT * FROM SaleMaster
WHERE ModifiedDate > '2026-09-24 15:00:00'
```

#### **Feature 2: Conflict Resolution**
```
If record exists in cloud:
  - Compare timestamps
  - Marg is always source of truth
  - Update cloud record
  - Log any discrepancies
```

#### **Feature 3: Error Handling**
```csharp
try {
    SyncProducts();
} catch (SqlException ex) {
    Log("Marg DB connection failed: " + ex.Message);
    Retry(3 times with exponential backoff);
}

try {
    UploadToCloud(data);
} catch (HttpException ex) {
    Log("API upload failed: " + ex.Message);
    SaveToLocalQueue(); // Upload later
}
```

#### **Feature 4: Bandwidth Optimization**
```
- Compress data before upload (gzip)
- Batch uploads (1000 records per API call)
- Upload during non-business hours (configurable)
- Skip unchanged records (hash comparison)
```

### 4.3 Installation Package

```
MargSyncUtility_Setup.exe
├── Check .NET 9 Runtime
├── Install Windows Service
├── Create desktop shortcut
├── Add to startup (optional)
├── Firewall exception (API access)
└── Create config folder: C:\ProgramData\InventoryProc\MargSync\
```

---

## 5. Sync Service Design

### 5.1 Backend Sync API

#### **Endpoint 1: Authenticate**
```http
POST /api/marg-sync/authenticate
Content-Type: application/json

{
  "shopCode": "SHOP001",
  "apiKey": "sk_live_abc123...",
  "margVersion": "9.0",
  "utilityVersion": "1.0.0"
}

Response:
{
  "success": true,
  "sessionToken": "xyz789...",
  "expiresIn": 86400,
  "shopId": "guid",
  "tenantId": "guid"
}
```

#### **Endpoint 2: Sync Products**
```http
POST /api/marg-sync/products
Authorization: Bearer {sessionToken}
Content-Type: application/json

{
  "shopCode": "SHOP001",
  "syncTimestamp": "2026-09-24T15:00:00Z",
  "products": [
    {
      "itemCode": "MED001",
      "itemName": "Paracetamol 500mg",
      "companyName": "Cipla",
      "packing": "10x10",
      "composition": "Paracetamol 500mg",
      "hsnCode": "30049099",
      "mrp": 50.00,
      "purchaseRate": 30.00,
      "saleRate": 45.00,
      "minStock": 100,
      "maxStock": 500,
      "scheduleType": null,
      "isActive": true,
      "groupName": "Analgesics",
      "batches": [
        {
          "batchNo": "B12345",
          "mfgDate": "2024-01-01",
          "expDate": "2026-12-31",
          "quantity": 250,
          "purchaseRate": 30.00,
          "mrp": 50.00
        }
      ]
    }
  ]
}

Response:
{
  "success": true,
  "productsCreated": 123,
  "productsUpdated": 45,
  "errors": []
}
```

#### **Endpoint 3: Sync Sales**
```http
POST /api/marg-sync/sales
Authorization: Bearer {sessionToken}

{
  "shopCode": "SHOP001",
  "syncTimestamp": "2026-09-24T15:00:00Z",
  "sales": [
    {
      "invNo": "INV-2024-001",
      "invDate": "2024-09-24",
      "partyCode": "CUST001",
      "partyName": "John Doe",
      "grossAmount": 1000.00,
      "taxAmount": 120.00,
      "discountAmount": 50.00,
      "netAmount": 1070.00,
      "paidAmount": 1070.00,
      "doctorName": "Dr. Smith",
      "paymentMode": "Cash",
      "items": [
        {
          "itemCode": "MED001",
          "itemName": "Paracetamol 500mg",
          "batchNo": "B12345",
          "quantity": 10,
          "rate": 45.00,
          "discount": 5.00,
          "taxPer": 12.00,
          "taxAmount": 48.00,
          "amount": 448.00
        }
      ]
    }
  ]
}
```

### 5.2 Data Transformation Layer

```csharp
public class MargToInventoryMapper
{
    public async Task<Product> MapProduct(MargItem margItem)
    {
        // Find or create category
        var category = await GetOrCreateCategory(margItem.GroupName);

        // Find or create brand
        var brand = await GetOrCreateBrand(margItem.CompanyName);

        return new Product(
            tenantId: _tenantId,
            name: margItem.ItemName,
            code: margItem.ItemCode,
            unit: "PCS", // Default or parse from packing
            purchasePrice: margItem.PurchaseRate,
            salePrice: margItem.SaleRate,
            mrp: margItem.MRP,
            categoryId: category.Id
        )
        {
            BrandId = brand.Id,
            Description = $"{margItem.Packing} - {margItem.Composition}",
            HSNCode = margItem.HSNCode,
            MinStockLevel = margItem.MinStock,
            MaxStockLevel = margItem.MaxStock,
            IsActive = margItem.IsActive
        };
    }

    public async Task<Invoice> MapSale(MargSale margSale)
    {
        // Find or create customer
        var customer = await GetOrCreateCustomer(
            margSale.PartyCode,
            margSale.PartyName
        );

        var invoice = new Invoice(
            tenantId: _tenantId,
            invoiceNumber: margSale.InvNo,
            customerId: customer.Id,
            customerName: margSale.PartyName,
            invoiceDate: margSale.InvDate,
            dueDate: margSale.InvDate.AddDays(customer.CreditDays)
        );

        // Map items
        foreach (var item in margSale.Items)
        {
            var product = await GetProductByCode(item.ItemCode);

            var invoiceItem = new InvoiceItem(
                productId: product.Id,
                productName: item.ItemName,
                productCode: item.ItemCode,
                quantity: item.Quantity,
                unit: "PCS",
                unitPrice: item.Rate,
                taxRate: item.TaxPer
            );

            invoice.AddItem(invoiceItem);
        }

        return invoice;
    }
}
```

---

## 6. Analytics & Reports

### 6.1 Medical Shop Specific Analytics

#### **Report 1: Expiry Dashboard**
```sql
-- Items expiring in next 30/60/90 days
SELECT
    p.Name AS MedicineName,
    b.BatchNo,
    b.ExpiryDate,
    b.Quantity,
    b.PurchaseRate * b.Quantity AS ValueAtRisk,
    DATEDIFF(day, GETDATE(), b.ExpiryDate) AS DaysToExpiry
FROM ProductBatch b
JOIN Products p ON b.ProductId = p.Id
WHERE b.ExpiryDate BETWEEN GETDATE() AND DATEADD(day, 90, GETDATE())
  AND b.Quantity > 0
ORDER BY b.ExpiryDate ASC
```

**Dashboard Visualization:**
- 🔴 Red Zone (0-30 days): Immediate action
- 🟡 Yellow Zone (31-60 days): Plan return
- 🟢 Green Zone (61-90 days): Monitor

#### **Report 2: Doctor-wise Sales**
```sql
-- Top prescribing doctors
SELECT
    DoctorName,
    COUNT(DISTINCT InvoiceId) AS PrescriptionCount,
    SUM(TotalAmount) AS TotalSales,
    AVG(TotalAmount) AS AvgPrescriptionValue
FROM Invoices
WHERE DoctorName IS NOT NULL
  AND InvoiceDate >= DATEADD(month, -3, GETDATE())
GROUP BY DoctorName
ORDER BY TotalSales DESC
```

#### **Report 3: Fast vs Slow Moving Items**
```sql
-- Fast moving (>10 sales/month)
-- Slow moving (<2 sales/month)
SELECT
    p.Name,
    p.Code,
    COUNT(ii.Id) AS SalesCount,
    SUM(ii.Quantity) AS TotalQuantity,
    p.CurrentStock,
    CASE
        WHEN COUNT(ii.Id) > 30 THEN 'Fast Moving'
        WHEN COUNT(ii.Id) BETWEEN 6 AND 30 THEN 'Average'
        ELSE 'Slow Moving'
    END AS MovementType
FROM Products p
LEFT JOIN InvoiceItems ii ON p.Id = ii.ProductId
LEFT JOIN Invoices i ON ii.InvoiceId = i.Id
WHERE i.InvoiceDate >= DATEADD(month, -3, GETDATE())
GROUP BY p.Id, p.Name, p.Code, p.CurrentStock
ORDER BY SalesCount DESC
```

#### **Report 4: Multi-Shop Comparison**
```sql
-- Compare sales across multiple shops
SELECT
    t.Name AS ShopName,
    COUNT(DISTINCT i.Id) AS TotalBills,
    SUM(i.TotalAmount) AS TotalSales,
    AVG(i.TotalAmount) AS AvgBillValue,
    SUM(i.TotalAmount - i.PaidAmount) AS OutstandingAmount
FROM Invoices i
JOIN Tenants t ON i.TenantId = t.Id
WHERE i.InvoiceDate >= DATEADD(month, -1, GETDATE())
GROUP BY t.Id, t.Name
ORDER BY TotalSales DESC
```

### 6.2 Web Dashboard Features

```
┌─────────────────────────────────────────────────────────────┐
│  InventoryProc Cloud Dashboard (view.inventory.com)        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Multi-Shop Overview:                                        │
│  ┌──────────┬──────────┬──────────┬──────────┐             │
│  │ Shop 1   │ Shop 2   │ Shop 3   │ Total    │             │
│  │ ₹45,230  │ ₹38,670  │ ₹52,100  │ ₹136,000 │             │
│  │ Today    │ Today    │ Today    │ Today    │             │
│  └──────────┴──────────┴──────────┴──────────┘             │
│                                                              │
│  ⚠️ Expiry Alerts:                                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │ 🔴 23 items expiring in 30 days (Value: ₹12,450)  │    │
│  │ 🟡 45 items expiring in 60 days (Value: ₹23,780)  │    │
│  │ [ View Details ]                                    │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  📊 Sales Trend (Last 30 Days):                            │
│  [Line Chart showing daily sales]                           │
│                                                              │
│  🏆 Top Selling Items:                                      │
│  1. Paracetamol 500mg - 450 units                          │
│  2. Crocin Advance - 320 units                             │
│  3. Dolo 650mg - 280 units                                 │
│                                                              │
│  💊 Stock Alerts:                                           │
│  - 12 items below minimum stock                            │
│  - 5 items near expiry need return                         │
│  [ View Recommendations ]                                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 7. Implementation Phases

### Phase 1: Foundation (Week 1-2)

**Backend:**
- [ ] Create Sync API endpoints
- [ ] Design ProductBatch table (for batch/expiry)
- [ ] Implement Marg data mappers
- [ ] Setup authentication & API keys

**Utility:**
- [ ] Create Windows Service project
- [ ] Implement Marg SQL Server connection
- [ ] Basic read operations (Products, Sales)
- [ ] Configuration UI

**Testing:**
- [ ] Test with sample Marg database
- [ ] Validate data transformation
- [ ] Error handling tests

### Phase 2: Core Sync (Week 3-4)

**Features:**
- [ ] Full product sync (with batches)
- [ ] Sales sync (last 90 days)
- [ ] Purchase sync
- [ ] Customer/Supplier sync
- [ ] Incremental sync logic
- [ ] Error recovery

**Utility:**
- [ ] Scheduling functionality
- [ ] Status dashboard
- [ ] Logging system
- [ ] System tray integration

### Phase 3: Analytics (Week 5-6)

**Reports:**
- [ ] Expiry dashboard
- [ ] Multi-shop consolidation
- [ ] Doctor analytics
- [ ] Fast/Slow moving items
- [ ] Profit analysis
- [ ] Stock optimization suggestions

**UI:**
- [ ] Cloud dashboard (Angular)
- [ ] Mobile-responsive reports
- [ ] Export to Excel/PDF
- [ ] Alerts and notifications

### Phase 4: Enhancement (Week 7-8)

**Advanced Features:**
- [ ] WhatsApp integration (receive orders)
- [ ] SMS alerts (expiry, stock)
- [ ] Automated reorder suggestions
- [ ] Supplier performance tracking
- [ ] Customer engagement (refill reminders)

---

## 8. Deployment Strategy

### 8.1 Utility Distribution

**Method 1: Direct Download**
```
https://inventory.com/download/marg-sync-utility
- Windows installer (.exe)
- User guide PDF
- Video tutorial
```

**Method 2: Remote Installation**
- TeamViewer/AnyDesk session
- Our team installs and configures
- Train shop staff
- Verify first sync

### 8.2 Onboarding Process

```
Step 1: Initial Contact
  ├── Demo the analytics dashboard
  ├── Show value (expiry alerts, multi-shop view)
  └── Get shop's consent

Step 2: Technical Setup
  ├── Schedule on-site visit
  ├── Get Marg database access
  ├── Install sync utility
  └── Configure sync schedule

Step 3: Data Migration
  ├── One-time full sync (historical data)
  ├── Verify data accuracy
  └── Compare reports with Marg

Step 4: Training
  ├── Show cloud dashboard
  ├── Explain reports
  └── Setup alerts

Step 5: Go Live
  ├── Start auto-sync
  ├── Monitor for 1 week
  └── Support calls
```

### 8.3 Pricing Model

**Option 1: Per Shop Pricing**
- ₹999/month per shop (single location)
- ₹2,499/month for 2-5 shops
- ₹4,999/month for 6-10 shops
- Enterprise (10+): Custom pricing

**Option 2: Transaction-based**
- Free up to 1000 bills/month
- ₹1,999/month for 1K-5K bills
- ₹3,999/month for 5K-10K bills

**Includes:**
- ✅ Unlimited sync
- ✅ Cloud storage (1 year history)
- ✅ All analytics reports
- ✅ Email/SMS alerts (100/month)
- ✅ Technical support

---

## 9. Technical Specifications

### 9.1 System Requirements

**Shop Computer (Sync Utility):**
- Windows 10/11
- .NET 9 Runtime
- 2 GB RAM minimum
- 500 MB disk space
- Internet connection (1 Mbps min)
- Marg ERP installed with SQL Server

**Cloud Server:**
- ASP.NET Core 9
- PostgreSQL 15+
- Redis (caching)
- 4 GB RAM minimum
- 50 GB storage per shop

### 9.2 Security

**Data Protection:**
- HTTPS/TLS 1.3 for all API calls
- API key authentication
- Data encrypted at rest (AES-256)
- Regular backups (daily)
- GDPR/India data compliance

**Access Control:**
- Role-based access (Owner, Manager, Accountant)
- IP whitelisting option
- Two-factor authentication
- Audit logs for all actions

---

## 10. Next Steps

### Immediate Actions

1. **Get Sample Marg Database** 🎯
   - Contact local medical shop
   - Get anonymized database dump
   - Study actual schema

2. **Build POC Sync Utility** 🎯
   - Simple console app
   - Read products from Marg
   - Upload to test API
   - Validate mapping

3. **Create Basic Dashboard** 🎯
   - Show synced data
   - Expiry alerts
   - Sales summary

4. **User Testing** 🎯
   - Demo to 2-3 friendly shops
   - Gather feedback
   - Iterate on features

---

## Appendix A: Marg Database Discovery Script

```sql
-- Run this in Marg SQL Server to discover schema
SELECT
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.name IN (
    'ItemMaster', 'BatchMaster', 'SaleMaster', 'SaleDetail',
    'PurchaseMaster', 'PurchaseDetail', 'PartyMaster', 'StockSummary'
)
ORDER BY t.name, c.column_id
```

---

**Document Version:** 1.0
**Last Updated:** September 24, 2026
**Status:** Ready for POC Development
