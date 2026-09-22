# Business Workflows & Flow Charts
## Wholesale/Distributor Business Management SaaS

**Document Version:** 1.0
**Date:** 2026-08-29
**Status:** Living Document
**Project:** InventoryProc - Wholesale Distributor SaaS MVP

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-08-29 | Development Team | Initial workflows document |

---

## Table of Contents

1. [Overview](#1-overview)
2. [Authentication Workflows](#2-authentication-workflows)
3. [Master Data Workflows](#3-master-data-workflows)
4. [Purchase Workflow](#4-purchase-workflow)
5. [Sales Workflow](#5-sales-workflow)
6. [Payment Workflow](#6-payment-workflow)
7. [Inventory Workflows](#7-inventory-workflows)
8. [Import Workflows](#8-import-workflows)
9. [Reporting Workflows](#9-reporting-workflows)
10. [Error Handling Flows](#10-error-handling-flows)

---

## 1. Overview

### 1.1 Workflow Notation

```
┌────────┐     = Process/Action
│ Action │
└────────┘

◇───────◇     = Decision Point
  Decision?

[Entity]      = Database Entity

→             = Flow Direction

║             = Parallel Processes

═══════       = Transaction Boundary
```

### 1.2 Core Business Loop

```
┌──────────────────────────────────────────────────────────┐
│                   CORE BUSINESS LOOP                      │
└──────────────────────────────────────────────────────────┘
                           │
                           ↓
              ┌────────────────────────┐
              │   Setup Master Data    │
              │  - Products            │
              │  - Customers           │
              │  - Suppliers           │
              └────────────┬───────────┘
                           │
                           ↓
              ┌────────────────────────┐
              │  Load Opening Stock    │
              └────────────┬───────────┘
                           │
           ┌───────────────┴───────────────┐
           │                               │
           ↓                               ↓
  ┌────────────────┐            ┌────────────────┐
  │   Purchase     │            │    Sales       │
  │  (Stock In)    │            │  (Stock Out)   │
  └────────┬───────┘            └────────┬───────┘
           │                               │
           │        ┌──────────┐           │
           └───────→│Inventory │←──────────┘
                    └─────┬────┘
                          │
                          ↓
                 ┌────────────────┐
                 │Customer Ledger │
                 │  Outstanding   │
                 └────────┬───────┘
                          │
                          ↓
                 ┌────────────────┐
                 │   Payments     │
                 │  (Reduce O/S)  │
                 └────────┬───────┘
                          │
                          ↓
                 ┌────────────────┐
                 │    Reports     │
                 │   Analytics    │
                 └────────────────┘
```

---

## 2. Authentication Workflows

### 2.1 User Registration Flow

```
┌─────────┐                                           ┌──────────┐
│  User   │                                           │  System  │
└────┬────┘                                           └─────┬────┘
     │                                                      │
     │  1. Submit Registration Form                        │
     │  (Email, Password, Name, Mobile)                    │
     ├──────────────────────────────────────────────────→  │
     │                                                      │
     │                                   2. Validate Input │
     │                          ◇─────────────────────◇    │
     │                            Invalid?  │  Valid        │
     │                                     │                │
     │  ←──────────────────────────────────┘                │
     │  Return Validation Errors                            │
     │                                                      │
     │                                      3. Hash Password│
     │                                      4. Create User  │
     │                                      [Users Table]   │
     │                                                      │
     │                                5. Generate JWT Token │
     │                                                      │
     │  ←──────────────────────────────────────────────────┤
     │  6. Return Token + User Info                         │
     │                                                      │
     │  7. Store Token (LocalStorage/SessionStorage)        │
     │                                                      │
     ↓                                                      ↓
```

### 2.2 User Login Flow

```
┌─────────┐                                           ┌──────────┐
│  User   │                                           │  System  │
└────┬────┘                                           └─────┬────┘
     │                                                      │
     │  1. Submit Login (Email + Password)                 │
     ├──────────────────────────────────────────────────→  │
     │                                                      │
     │                               2. Find User by Email │
     │                                      [Users Table]   │
     │                                                      │
     │                           ◇─────────────────◇       │
     │                             User Exists?             │
     │                                No  │  Yes            │
     │  ←─────────────────────────────────┘                │
     │  Return "Invalid Credentials"                        │
     │                                                      │
     │                                   3. Verify Password │
     │                           ◇─────────────────◇       │
     │                             Password OK?             │
     │                                No  │  Yes            │
     │  ←─────────────────────────────────┘                │
     │  Return "Invalid Credentials"                        │
     │                                                      │
     │                              4. Check User IsActive  │
     │                           ◇─────────────────◇       │
     │                                Active?               │
     │                                No  │  Yes            │
     │  ←─────────────────────────────────┘                │
     │  Return "Account Deactivated"                        │
     │                                                      │
     │                                5. Generate JWT Token │
     │                                   (with TenantId)    │
     │                                                      │
     │  ←──────────────────────────────────────────────────┤
     │  6. Return Token + User Info + Tenant Info           │
     │                                                      │
     │  7. Store Token + Redirect to Dashboard              │
     │                                                      │
     ↓                                                      ↓
```

### 2.3 Request Authorization Flow

```
Every API Request
       │
       ↓
┌──────────────────┐
│ Extract JWT Token│
│ from Header      │
└────────┬─────────┘
         │
         ↓
    ◇────────◇
     Token
     Valid?
         │ No
         ├─────→ Return 401 Unauthorized
         │
         │ Yes
         ↓
┌──────────────────┐
│ Extract TenantId │
│ from Token       │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ Set Current      │
│ Tenant Context   │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ Check User Role  │
│ & Permissions    │
└────────┬─────────┘
         │
         ↓
    ◇────────◇
     Authorized?
         │ No
         ├─────→ Return 403 Forbidden
         │
         │ Yes
         ↓
┌──────────────────┐
│ Execute Request  │
│ (Auto-filter by  │
│  TenantId)       │
└────────┬─────────┘
         │
         ↓
   Return Response
```

---

## 3. Master Data Workflows

### 3.1 Product Creation Flow

```
┌─────────┐                                           ┌──────────┐
│  User   │                                           │  System  │
└────┬────┘                                           └─────┬────┘
     │                                                      │
     │  1. Fill Product Form                               │
     │  (Name, SKU, Price, Category, etc.)                 │
     ├──────────────────────────────────────────────────→  │
     │                                                      │
     │                                   2. Validate Input │
     │                          ◇─────────────────────◇    │
     │                            Invalid?  │  Valid        │
     │                                     │                │
     │  ←──────────────────────────────────┘                │
     │  Return Validation Errors                            │
     │                                                      │
     │                         3. Check SKU Uniqueness     │
     │                            (within Tenant)           │
     │                                                      │
     │                          ◇─────────────────────◇    │
     │                            Duplicate? │  Unique      │
     │                                      │               │
     │  ←───────────────────────────────────┘               │
     │  Return "SKU already exists"                         │
     │                                                      │
     │  ═══════════ BEGIN TRANSACTION ═══════════          │
     │                                                      │
     │                               4. Auto-set TenantId  │
     │                               5. Create Product     │
     │                                  [Products Table]    │
     │                                                      │
     │                           6. Create Stock Balance   │
     │                              (Quantity = 0)          │
     │                              [StockBalances Table]   │
     │                                                      │
     │  ═══════════ COMMIT TRANSACTION ═══════════         │
     │                                                      │
     │  ←──────────────────────────────────────────────────┤
     │  7. Return Created Product                           │
     │                                                      │
     ↓                                                      ↓
```

### 3.2 Customer Creation Flow

```
User Input
    │
    ↓
┌──────────────────┐
│ Validate Input   │
│ - Required fields│
│ - Valid mobile   │
│ - Valid GST      │
└────────┬─────────┘
         │
         ↓
    ◇────────◇
     Valid?
         │ No → Return Errors
         │
         │ Yes
         ↓
═══ BEGIN TRANSACTION ═══
         │
         ↓
┌──────────────────┐
│ Create Customer  │
│ [Customers Table]│
└────────┬─────────┘
         │
         ↓
    ◇────────◇
     Opening
    Balance > 0?
         │ No → Skip
         │
         │ Yes
         ↓
┌────────────────────────┐
│ Create Ledger Entry    │
│ Type: OpeningBalance   │
│ Debit: OpeningBalance  │
│ [CustomerLedgerEntries]│
└────────┬───────────────┘
         │
═══ COMMIT TRANSACTION ═══
         │
         ↓
   Return Customer
```

---

## 4. Purchase Workflow

### 4.1 Complete Purchase Flow

```
┌────────────────────────────────────────────────────────────┐
│                    PURCHASE WORKFLOW                        │
└────────────────────────────────────────────────────────────┘

User Action: Create Purchase
         │
         ↓
┌──────────────────┐
│ 1. Select        │
│    Supplier      │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 2. Enter Invoice │
│    Number & Date │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 3. Add Line Items│
│    - Select      │
│      Product     │
│    - Enter Qty   │
│    - Enter Price │
│    - Discount/Tax│
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 4. Calculate     │
│    Totals        │
│    (Client Side) │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 5. Submit        │
│    Purchase      │
└────────┬─────────┘
         │
         ↓
    ════════════════════════════
    ║  SERVER-SIDE PROCESSING  ║
    ════════════════════════════
         │
         ↓
┌──────────────────────────────┐
│ 6. Validate Input            │
│    - Required fields         │
│    - Positive quantities     │
│    - Valid supplier          │
│    - Valid products          │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Valid?
         │ No → Return 400 Bad Request
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 7. Check Invoice Number      │
│    Uniqueness (within Tenant)│
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
    Duplicate?
         │ Yes → Return 409 Conflict
         │
         │ No
         ↓
┌──────────────────────────────┐
│ 8. Recalculate Totals        │
│    (Server-side, don't       │
│     trust client)            │
└────────┬─────────────────────┘
         │
         ↓
═══════ BEGIN TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 9. Create Purchase           │
│    [Purchases Table]         │
│    - TenantId (auto-set)     │
│    - SupplierId              │
│    - InvoiceNumber           │
│    - Totals (server calc)    │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 10. Create Purchase Items    │
│     [PurchaseItems Table]    │
│     - For each line item     │
│     - ProductId, Qty, Price  │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 11. Update Inventory         │
│     FOR EACH ITEM:           │
│                              │
│     a) Increase Stock        │
│        [StockBalances Table] │
│        QuantityOnHand += Qty │
│                              │
│     b) Create Transaction    │
│        [InventoryTransactions│
│        Type: Purchase        │
│        Quantity: +Qty        │
│        ReferenceId: PurchaseId│
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     All
   Updates OK?
         │ No → ROLLBACK
         │      Return 500
         │
         │ Yes
         ↓
═══════ COMMIT TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 12. Return Success           │
│     - Purchase ID            │
│     - Grand Total            │
│     - Items Count            │
└────────┬─────────────────────┘
         │
         ↓
   Purchase Complete
   Stock Updated ✓
```

### 4.2 Purchase-Inventory Integration

```
Purchase Created
       │
       ↓
┌──────────────────────────────────┐
│ For Each Purchase Item:          │
│                                  │
│  Product A: Qty = 100            │
│  Product B: Qty = 50             │
└────────┬─────────────────────────┘
         │
         ├───────────────┬──────────────────┐
         │               │                  │
         ↓               ↓                  ↓
    Product A        Product B          Product N
         │               │                  │
         ↓               ↓                  ↓
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ Update Stock    │ │ Update Stock    │ │ Update Stock    │
│ Balance:        │ │ Balance:        │ │ Balance:        │
│ Old: 200        │ │ Old: 80         │ │ Old: X          │
│ Add: 100        │ │ Add: 50         │ │ Add: Y          │
│ New: 300        │ │ New: 130        │ │ New: X+Y        │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │               │                  │
         ↓               ↓                  ↓
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ Create Inv Txn  │ │ Create Inv Txn  │ │ Create Inv Txn  │
│ Type: Purchase  │ │ Type: Purchase  │ │ Type: Purchase  │
│ Qty: +100       │ │ Qty: +50        │ │ Qty: +Y         │
│ Ref: PurchaseId │ │ Ref: PurchaseId │ │ Ref: PurchaseId │
└─────────────────┘ └─────────────────┘ └─────────────────┘

All operations must succeed or entire purchase is rolled back
```

---

## 5. Sales Workflow

### 5.1 Complete Sales Flow with Validations

```
┌────────────────────────────────────────────────────────────┐
│                     SALES WORKFLOW                          │
└────────────────────────────────────────────────────────────┘

User Action: Create Sale
         │
         ↓
┌──────────────────┐
│ 1. Select        │
│    Customer      │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 2. Add Products  │
│    (Multiple)    │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 3. For Each      │
│    Product:      │
│    a) Check Stock│
│       Availability│
│    b) Enter Qty  │
│    c) Set Price  │
│    d) Discount   │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 4. Enter Payment │
│    Amount        │
│    (Optional)    │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 5. Submit Sale   │
└────────┬─────────┘
         │
         ↓
    ════════════════════════════
    ║  SERVER-SIDE PROCESSING  ║
    ════════════════════════════
         │
         ↓
┌──────────────────────────────┐
│ 6. Validate Input            │
│    - Customer exists & active│
│    - Products exist & active │
│    - Quantities > 0          │
│    - PaidAmount <= GrandTotal│
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Valid?
         │ No → Return 400 Bad Request
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 7. Validate Stock            │
│    FOR EACH ITEM:            │
│    Check Available Stock     │
│    >= Requested Quantity     │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Stock
   Available?
         │ No → Return 422
         │      "Insufficient Stock"
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 8. Check Customer Credit     │
│    (If configured)           │
│    CurrentOutstanding +      │
│    NewSaleAmount             │
│    <= CreditLimit?           │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Within
     Limit?
         │ No → Return 422
         │      "Credit Limit Exceeded"
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 9. Recalculate Totals        │
│    (Server-side)             │
│    Calculate PaymentStatus   │
└────────┬─────────────────────┘
         │
         ↓
═══════ BEGIN TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 10. Create Sale              │
│     [Sales Table]            │
│     - TenantId (auto-set)    │
│     - CustomerId             │
│     - Totals (server calc)   │
│     - PaymentStatus          │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 11. Create Sale Items        │
│     [SaleItems Table]        │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 12. Update Inventory         │
│     FOR EACH ITEM:           │
│                              │
│     a) Decrease Stock        │
│        [StockBalances]       │
│        QuantityOnHand -= Qty │
│                              │
│     b) Create Transaction    │
│        [InventoryTransactions│
│        Type: Sale            │
│        Quantity: -Qty        │
│        ReferenceId: SaleId   │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 13. Create Customer Ledger   │
│     Entry (Debit)            │
│     [CustomerLedgerEntries]  │
│     Type: Sale               │
│     Debit: GrandTotal        │
│     RunningBalance updated   │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
    PaidAmount
       > 0?
         │ No → Skip to Commit
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 14. Create Payment           │
│     [Payments Table]         │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 15. Create Ledger Entry      │
│     (Credit)                 │
│     Type: Payment            │
│     Credit: PaidAmount       │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     All
   Updates OK?
         │ No → ROLLBACK
         │      Return 500
         │
         │ Yes
         ↓
═══════ COMMIT TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 16. Return Success           │
│     - Sale ID                │
│     - Invoice Number         │
│     - Grand Total            │
│     - Payment Status         │
└────────┬─────────────────────┘
         │
         ↓
   Sale Complete ✓
   Stock Decreased ✓
   Ledger Updated ✓
```

### 5.2 Sales Payment Status Logic

```
After Sale Creation:

DueAmount = GrandTotal - PaidAmount

┌────────────────────────┐
│  Determine Payment     │
│  Status                │
└───────────┬────────────┘
            │
            ↓
       ◇────────◇
      PaidAmount
         = 0?
            │ Yes → PaymentStatus = "Unpaid"
            │
            │ No
            ↓
       ◇────────◇
      PaidAmount
    = GrandTotal?
            │ Yes → PaymentStatus = "Paid"
            │
            │ No
            ↓
       ◇────────◇
      0 < PaidAmount
     < GrandTotal?
            │ Yes → PaymentStatus = "Partial"
            │
            ↓
     PaymentStatus Set
```

---

## 6. Payment Workflow

### 6.1 Payment Recording Flow

```
┌────────────────────────────────────────────────────────────┐
│                   PAYMENT WORKFLOW                          │
└────────────────────────────────────────────────────────────┘

User Action: Record Payment
         │
         ↓
┌──────────────────┐
│ 1. Select        │
│    Customer      │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 2. View Current  │
│    Outstanding   │
│    Balance       │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 3. Enter Payment │
│    Details:      │
│    - Amount      │
│    - Date        │
│    - Method      │
│    - Reference   │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 4. Submit        │
│    Payment       │
└────────┬─────────┘
         │
         ↓
    ════════════════════════════
    ║  SERVER-SIDE PROCESSING  ║
    ════════════════════════════
         │
         ↓
┌──────────────────────────────┐
│ 5. Validate Input            │
│    - Customer exists & active│
│    - Amount > 0              │
│    - Valid payment method    │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Valid?
         │ No → Return 400 Bad Request
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 6. Get Customer Outstanding  │
│    Balance                   │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
    Payment >
   Outstanding?
         │ Yes → Allow (will create
         │       credit balance)
         │
         │ No (Normal case)
         ↓
═══════ BEGIN TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 7. Create Payment            │
│    [Payments Table]          │
│    - TenantId (auto-set)     │
│    - CustomerId              │
│    - Amount                  │
│    - PaymentMethod           │
│    - ReferenceNumber         │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 8. Create Ledger Entry       │
│    (Credit)                  │
│    [CustomerLedgerEntries]   │
│    Type: Payment             │
│    Credit: Amount            │
│    RunningBalance -= Amount  │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 9. Update Sale(s) Payment    │
│    Status (if applicable)    │
│    - Match oldest unpaid     │
│      sales first (FIFO)      │
│    - Update PaymentStatus    │
│    - Update DueAmount        │
└────────┬─────────────────────┘
         │
         ↓
═══════ COMMIT TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 10. Return Success           │
│     - Payment ID             │
│     - New Outstanding Balance│
└────────┬─────────────────────┘
         │
         ↓
   Payment Recorded ✓
   Outstanding Updated ✓
```

### 6.2 Customer Ledger Update Flow

```
Customer Ledger Running Balance Calculation:

Initial State:
┌────────────────────────────┐
│ Opening Balance: 5,000     │  (Debit - Customer owes)
└────────────────────────────┘

Transaction 1: Sale
┌────────────────────────────┐
│ Date: Aug 1                │
│ Type: Sale                 │
│ Invoice: INV-001           │
│ Debit: 10,000              │
│ RunningBalance: 15,000     │  (5,000 + 10,000)
└────────────────────────────┘

Transaction 2: Payment
┌────────────────────────────┐
│ Date: Aug 5                │
│ Type: Payment              │
│ Reference: PAY-001         │
│ Credit: 8,000              │
│ RunningBalance: 7,000      │  (15,000 - 8,000)
└────────────────────────────┘

Transaction 3: Sale
┌────────────────────────────┐
│ Date: Aug 10               │
│ Type: Sale                 │
│ Invoice: INV-002           │
│ Debit: 5,000               │
│ RunningBalance: 12,000     │  (7,000 + 5,000)
└────────────────────────────┘

Transaction 4: Payment
┌────────────────────────────┐
│ Date: Aug 15               │
│ Type: Payment              │
│ Reference: PAY-002         │
│ Credit: 2,000              │
│ RunningBalance: 10,000     │  (12,000 - 2,000)
└────────────────────────────┘

Current Outstanding: 10,000 (Debit balance)
```

---

## 7. Inventory Workflows

### 7.1 Opening Stock Entry Flow

```
User Action: Import Opening Stock
         │
         ↓
┌──────────────────┐
│ 1. Upload Excel  │
│    File          │
└────────┬─────────┘
         │
         ↓
┌──────────────────────────────┐
│ 2. Validate File Format      │
│    - Check columns           │
│    - Check data types        │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Valid?
         │ No → Return Errors
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 3. Validate Each Row         │
│    - Product exists          │
│    - Quantity > 0            │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 4. Preview Results           │
│    - Valid rows: Green       │
│    - Invalid rows: Red       │
│      with error messages     │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────┐
│ 5. User Confirms │
│    Import        │
└────────┬─────────┘
         │
         ↓
═══════ BEGIN TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 6. For Each Valid Row:       │
│                              │
│    a) Update Stock Balance   │
│       [StockBalances]        │
│       QuantityOnHand += Qty  │
│                              │
│    b) Create Inv Transaction │
│       [InventoryTransactions]│
│       Type: OpeningStock     │
│       Quantity: +Qty         │
└────────┬─────────────────────┘
         │
         ↓
═══════ COMMIT TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 7. Return Results            │
│    - Imported: X rows        │
│    - Skipped: Y rows         │
│    - Errors: Z rows          │
└──────────────────────────────┘
```

### 7.2 Stock Validation Flow (Before Sale)

```
Sale Attempt
     │
     ↓
For Each Sale Item:
     │
     ↓
┌──────────────────────────────┐
│ 1. Get Product Stock Balance │
│    [StockBalances Table]     │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 2. Calculate Available Stock │
│    Available = QuantityOnHand│
│              - ReservedQty   │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
    Available
      >=
   Requested?
         │ No → ┌──────────────────────┐
         │      │ Return Error:        │
         │      │ "Insufficient stock  │
         │      │  for Product X"      │
         │      │ "Available: A"       │
         │      │ "Requested: R"       │
         │      └──────────────────────┘
         │
         │ Yes → Continue to next item
         │
         ↓
   All Items Valid?
         │ Yes → Proceed with Sale
         │
         │ No → Return all errors
         │
         ↓
```

---

## 8. Import Workflows

### 8.1 Excel Import Flow (Generic)

```
┌────────────────────────────────────────────────────────────┐
│                   EXCEL IMPORT WORKFLOW                     │
└────────────────────────────────────────────────────────────┘

User Action: Import Data
         │
         ↓
┌──────────────────┐
│ 1. Download      │
│    Template      │
│    (Optional)    │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 2. Fill Excel    │
│    with Data     │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ 3. Upload File   │
└────────┬─────────┘
         │
         ↓
    ════════════════════════════
    ║  SERVER-SIDE PROCESSING  ║
    ════════════════════════════
         │
         ↓
┌──────────────────────────────┐
│ PHASE 1: File Validation     │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 4. Validate File             │
│    - Is Excel format?        │
│    - File size OK?           │
│    - Not corrupted?          │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Valid?
         │ No → Return Error
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 5. Read Excel Sheets         │
│    Extract rows              │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 6. Validate Column Headers   │
│    Expected vs Actual        │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Columns
      Match?
         │ No → Return Error with
         │      missing/extra columns
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ PHASE 2: Row Validation      │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 7. For Each Row:             │
│                              │
│    a) Validate Required      │
│       Fields                 │
│                              │
│    b) Validate Data Types    │
│       (number, date, email)  │
│                              │
│    c) Validate Business      │
│       Rules                  │
│       - Unique constraints   │
│       - FK references        │
│       - Value ranges         │
│                              │
│    d) Collect Errors         │
│       with Row Numbers       │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 8. Categorize Rows           │
│    - Valid rows: []          │
│    - Invalid rows: []        │
│      with error details      │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 9. Generate Preview          │
│    Show:                     │
│    - Total rows              │
│    - Valid count             │
│    - Invalid count           │
│    - Errors list             │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 10. Return Preview to User   │
│     Wait for Confirmation    │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     User
    Confirms?
         │ No → Cancel Import
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ PHASE 3: Data Import         │
└────────┬─────────────────────┘
         │
         ↓
═══════ BEGIN TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 11. For Each Valid Row:      │
│                              │
│     a) Map Excel Data to DTO │
│                              │
│     b) Create/Update Entity  │
│                              │
│     c) Related Updates       │
│        (e.g., stock balance) │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     All
   Successful?
         │ No → ROLLBACK
         │      Return Error
         │
         │ Yes
         ↓
═══════ COMMIT TRANSACTION ═══════
         │
         ↓
┌──────────────────────────────┐
│ 12. Return Final Results     │
│     - Imported: X rows       │
│     - Failed: Y rows         │
│     - Skipped: Z rows        │
│     - Error details          │
└────────┬─────────────────────┘
         │
         ↓
   Import Complete ✓
```

---

## 9. Reporting Workflows

### 9.1 Dashboard Loading Flow

```
User Opens Dashboard
         │
         ↓
┌──────────────────────────────┐
│ 1. Request Dashboard Data    │
│    Default: Current Month    │
└────────┬─────────────────────┘
         │
         ↓
    ════════════════════════════
    ║  SERVER-SIDE QUERIES     ║
    ║  (Parallel Execution)    ║
    ════════════════════════════
         │
    ┌────┼────┬────┬────┬────┬────┐
    │    │    │    │    │    │    │
    ↓    ↓    ↓    ↓    ↓    ↓    ↓
┌─────┐┌─────┐┌─────┐┌─────┐┌─────┐
│Query││Query││Query││Query││Query│
│ 1  ││  2  ││  3  ││  4  ││  5  │
└──┬──┘└──┬──┘└──┬──┘└──┬──┘└──┬──┘
   │      │      │      │      │
   ↓      ↓      ↓      ↓      ↓

Query 1: Today's Sales
┌──────────────────────────────┐
│ SELECT                       │
│   COUNT(*) as Count,         │
│   SUM(GrandTotal) as Total   │
│ FROM Sales                   │
│ WHERE TenantId = @TenantId   │
│   AND SaleDate = @Today      │
└──────────────────────────────┘

Query 2: Month's Sales
┌──────────────────────────────┐
│ SELECT                       │
│   COUNT(*), SUM(GrandTotal)  │
│ FROM Sales                   │
│ WHERE TenantId = @TenantId   │
│   AND SaleDate >= @MonthStart│
└──────────────────────────────┘

Query 3: Total Outstanding
┌──────────────────────────────┐
│ SELECT                       │
│   SUM(RunningBalance)        │
│ FROM CustomerLedgerEntries   │
│ WHERE TenantId = @TenantId   │
│ GROUP BY CustomerId          │
│ HAVING RunningBalance > 0    │
└──────────────────────────────┘

Query 4: Low Stock Count
┌──────────────────────────────┐
│ SELECT COUNT(*)              │
│ FROM StockBalances sb        │
│ JOIN Products p ON sb.ProductId│
│ WHERE p.TenantId = @TenantId │
│   AND sb.QuantityOnHand      │
│     < p.MinimumStock         │
└──────────────────────────────┘

Query 5: Top Customers
┌──────────────────────────────┐
│ SELECT TOP 5                 │
│   CustomerId,                │
│   CustomerName,              │
│   SUM(GrandTotal) as Total   │
│ FROM Sales                   │
│ WHERE TenantId = @TenantId   │
│   AND SaleDate >= @MonthStart│
│ GROUP BY CustomerId          │
│ ORDER BY Total DESC          │
└──────────────────────────────┘

    │      │      │      │      │
    └──────┴───┬──┴──────┴──────┘
               │
               ↓
┌──────────────────────────────┐
│ 2. Aggregate Results         │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 3. Return Dashboard DTO      │
│    {                         │
│      todaySales,             │
│      monthSales,             │
│      totalOutstanding,       │
│      lowStockCount,          │
│      topCustomers,           │
│      recentSales,            │
│      topProducts             │
│    }                         │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 4. Frontend Renders          │
│    Dashboard Widgets         │
└──────────────────────────────┘
```

### 9.2 Sales Report Flow

```
User Requests Sales Report
         │
         ↓
┌──────────────────────────────┐
│ 1. Select Report Type        │
│    - Sales Summary           │
│    - Sales by Product        │
│    - Sales by Customer       │
│    - Sales Detail            │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 2. Apply Filters             │
│    - Date Range (required)   │
│    - Customer (optional)     │
│    - Product (optional)      │
│    - Category (optional)     │
│    - Payment Status (opt)    │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 3. Request Report            │
│    (with filters)            │
└────────┬─────────────────────┘
         │
         ↓
    ════════════════════════════
    ║  SERVER-SIDE PROCESSING  ║
    ════════════════════════════
         │
         ↓
┌──────────────────────────────┐
│ 4. Build Query               │
│    - Base query for report   │
│    - Apply tenant filter     │
│    - Apply date range        │
│    - Apply optional filters  │
│    - Add sorting             │
│    - Add pagination          │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 5. Execute Query             │
│    (Optimized with indexes)  │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 6. Calculate Aggregates      │
│    - Totals                  │
│    - Counts                  │
│    - Averages                │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 7. Format Results            │
│    - Map to DTOs             │
│    - Include pagination info │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 8. Return Report Data        │
└────────┬─────────────────────┘
         │
         ↓
┌──────────────────────────────┐
│ 9. Frontend Displays Report  │
│    - Table view              │
│    - Charts (optional)       │
│    - Export buttons          │
└────────┬─────────────────────┘
         │
         ↓
    ◇────────◇
     Export?
         │ No → End
         │
         │ Yes
         ↓
┌──────────────────────────────┐
│ 10. Export Report            │
│     - Excel (data)           │
│     - PDF (formatted)        │
└──────────────────────────────┘
```

---

## 10. Error Handling Flows

### 10.1 Global Error Handling Flow

```
Any API Request
       │
       ↓
   Try {
       │
       ↓
   Execute Logic
       │
   }
   ↓
Catch (Exception ex)
   │
   ↓
┌──────────────────────────────┐
│ Exception Type?              │
└────────┬─────────────────────┘
         │
    ┌────┼────┬────┬────┬────┐
    │    │    │    │    │    │
    ↓    ↓    ↓    ↓    ↓    ↓

ValidationException
    ↓
┌──────────────────┐
│ Return 400       │
│ Bad Request      │
│ - Field errors   │
│ - Error messages │
└──────────────────┘

NotFoundException
    ↓
┌──────────────────┐
│ Return 404       │
│ Not Found        │
│ - Resource type  │
│ - Resource ID    │
└──────────────────┘

BusinessRuleException
    ↓
┌──────────────────┐
│ Return 422       │
│ Unprocessable    │
│ - Rule violated  │
│ - Clear message  │
└──────────────────┘

UnauthorizedException
    ↓
┌──────────────────┐
│ Return 401       │
│ Unauthorized     │
│ - Token invalid  │
└──────────────────┘

ForbiddenException
    ↓
┌──────────────────┐
│ Return 403       │
│ Forbidden        │
│ - Insufficient   │
│   permissions    │
└──────────────────┘

DbUpdateException
    ↓
┌──────────────────┐
│ Check Inner      │
│ Exception        │
└────────┬─────────┘
         │
    ◇────────◇
    Unique
   Constraint?
         │ Yes
         ↓
   ┌──────────────────┐
   │ Return 409       │
   │ Conflict         │
   │ - Duplicate key  │
   └──────────────────┘
         │ No
         ↓
   ┌──────────────────┐
   │ Return 500       │
   │ Database Error   │
   └──────────────────┘

All Other Exceptions
    ↓
┌──────────────────┐
│ Log Error        │
│ (Full stack)     │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ Return 500       │
│ Internal Server  │
│ Error            │
│ - Generic msg    │
│ - Request ID     │
└──────────────────┘
```

### 10.2 Transaction Rollback Flow

```
BEGIN TRANSACTION
       │
       ↓
   Try {
       │
       ↓
   ┌──────────────┐
   │ Operation 1  │
   │   Success    │
   └──────┬───────┘
          │
          ↓
   ┌──────────────┐
   │ Operation 2  │
   │   Success    │
   └──────┬───────┘
          │
          ↓
   ┌──────────────┐
   │ Operation 3  │
   │   FAILED     │ ← Error occurs here
   └──────┬───────┘
          │
   }      │
   Catch (Exception)
       │
       ↓
┌──────────────────┐
│ ROLLBACK         │
│ TRANSACTION      │
│                  │
│ All changes      │
│ discarded        │
│                  │
│ Database state   │
│ returned to      │
│ beginning of     │
│ transaction      │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ Log Error        │
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│ Return Error     │
│ Response to User │
└──────────────────┘

Example:
Sale Creation with Stock Update

BEGIN TRANSACTION
  1. Create Sale ✓
  2. Create Sale Items ✓
  3. Update Stock Balance ✓
  4. Create Inventory Transaction ✓
  5. Create Customer Ledger Entry ✗ ← FAILED

ROLLBACK → Everything undone
- Sale not created
- Sale items not created
- Stock not updated
- No inventory transaction
- No ledger entry

User receives error message with details
```

---

## Appendix A: Workflow Sequence Examples

### Complete Sale Example (Happy Path)

```
Time: 10:00 AM - Sales Staff creates a sale

1. Login ✓
2. Navigate to Sales → New Sale
3. Search Customer: "Retail Shop A" ✓
4. Add Product: "Product A", Qty: 10
   - System checks stock: 250 available ✓
5. Add Product: "Product B", Qty: 5
   - System checks stock: 80 available ✓
6. System calculates totals:
   - SubTotal: 2,500
   - Tax: 250
   - GrandTotal: 2,750
7. Enter PaidAmount: 1,000
   - PaymentMethod: Cash
8. Submit Sale
9. System validates:
   - Customer active ✓
   - Stock available ✓
   - Paid <= Grand Total ✓
10. System creates:
    - Sale record (INV-123)
    - 2 Sale items
    - Decreases stock (Product A: 240, Product B: 75)
    - 2 Inventory transactions
    - Customer ledger debit: 2,750
    - Payment record: 1,000
    - Customer ledger credit: 1,000
11. Return success:
    - Invoice Number: INV-123
    - Grand Total: 2,750
    - Payment Status: Partial
    - Due Amount: 1,750
12. User prints invoice
13. Customer receives invoice

Total Time: ~2 minutes
Database Operations: 9 inserts, 2 updates
Transaction: ACID compliant
```

---

**Document Status:** This is a living document and will be updated as workflows evolve during implementation.
