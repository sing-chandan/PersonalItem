# API Specification
## Wholesale/Distributor Business Management SaaS

**Document Version:** 1.0
**Date:** 2026-08-29
**Status:** Living Document
**Project:** InventoryProc - Wholesale Distributor SaaS MVP

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-08-29 | Development Team | Initial API specification |

---

## Table of Contents

1. [API Overview](#1-api-overview)
2. [Authentication](#2-authentication)
3. [Common Patterns](#3-common-patterns)
4. [Error Handling](#4-error-handling)
5. [API Endpoints](#5-api-endpoints)
6. [Request/Response Examples](#6-requestresponse-examples)

---

## 1. API Overview

### 1.1 Base URL

```
Development:  http://localhost:5000/api
Staging:      https://staging.inventoryproc.com/api
Production:   https://api.inventoryproc.com/api
```

### 1.2 API Style

- **Protocol:** HTTPS (HTTP in development only)
- **Format:** REST/JSON
- **Authentication:** JWT Bearer Token
- **Versioning:** URL-based (e.g., `/api/v1/...`) - Currently v1 is implicit

### 1.3 Request Headers

**Required Headers:**
```http
Content-Type: application/json
Authorization: Bearer {jwt_token}
```

**Optional Headers:**
```http
Accept-Language: en-US
X-Request-Id: {unique_request_id}
```

### 1.4 Response Headers

```http
Content-Type: application/json
X-Request-Id: {unique_request_id}
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1672531200
```

---

## 2. Authentication

### 2.1 Authentication Flow

```
1. User Registration (POST /api/auth/register)
2. User Login (POST /api/auth/login)
3. Receive JWT Token
4. Include Token in all subsequent requests: Authorization: Bearer {token}
5. Token Refresh (POST /api/auth/refresh) - before expiry
```

### 2.2 JWT Token Structure

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

### 2.3 Token Expiration

- **Access Token:** 24 hours
- **Refresh Token:** 30 days

---

## 3. Common Patterns

### 3.1 Pagination

**Query Parameters:**
```
?page=1&pageSize=20
```

**Response Structure:**
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

### 3.2 Filtering

```
?search=keyword
?categoryId=guid
&isActive=true
&fromDate=2026-01-01
&toDate=2026-12-31
```

### 3.3 Sorting

```
?sortBy=name&sortOrder=asc
?sortBy=createdAt&sortOrder=desc
```

### 3.4 Standard Response Wrapper

**Success Response:**
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

**Error Response:**
```json
{
  "success": false,
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ],
  "message": "Validation failed"
}
```

---

## 4. Error Handling

### 4.1 HTTP Status Codes

| Code | Description | Usage |
|------|-------------|-------|
| **200** | OK | Successful GET, PUT, PATCH |
| **201** | Created | Successful POST (resource created) |
| **204** | No Content | Successful DELETE |
| **400** | Bad Request | Validation errors, malformed request |
| **401** | Unauthorized | Missing or invalid authentication |
| **403** | Forbidden | Insufficient permissions |
| **404** | Not Found | Resource not found |
| **409** | Conflict | Duplicate resource (SKU, invoice number) |
| **422** | Unprocessable Entity | Business rule violation |
| **500** | Internal Server Error | Unexpected server error |

### 4.2 Error Response Format

```json
{
  "success": false,
  "errorCode": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "errors": [
    {
      "field": "email",
      "message": "Email is required",
      "code": "REQUIRED"
    },
    {
      "field": "price",
      "message": "Price must be greater than zero",
      "code": "GREATER_THAN_ZERO"
    }
  ],
  "timestamp": "2026-08-29T10:30:00Z",
  "requestId": "abc-123-def"
}
```

### 4.3 Common Error Codes

| Error Code | Description |
|-----------|-------------|
| **VALIDATION_ERROR** | Request validation failed |
| **NOT_FOUND** | Resource not found |
| **DUPLICATE** | Resource already exists (unique constraint) |
| **INSUFFICIENT_STOCK** | Not enough stock for sale |
| **BUSINESS_RULE_VIOLATION** | Business logic constraint violated |
| **UNAUTHORIZED** | Invalid or missing authentication |
| **FORBIDDEN** | Insufficient permissions |
| **TENANT_MISMATCH** | Resource belongs to different tenant |

---

## 5. API Endpoints

### 5.1 Authentication & Identity

#### POST /api/auth/register
**Description:** Register a new user for a tenant
**Authorization:** Public (tenant admin registration)

**Request:**
```json
{
  "tenantCode": "DEMO001",
  "email": "admin@example.com",
  "password": "SecureP@ss123",
  "firstName": "John",
  "lastName": "Doe",
  "mobile": "+911234567890"
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "tenantId": "guid",
    "email": "admin@example.com",
    "token": "jwt_token",
    "refreshToken": "refresh_token"
  }
}
```

---

#### POST /api/auth/login
**Description:** User login
**Authorization:** Public

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "tenantId": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Admin",
    "token": "jwt_token",
    "refreshToken": "refresh_token",
    "expiresAt": "2026-08-30T10:30:00Z"
  }
}
```

---

#### POST /api/auth/refresh
**Description:** Refresh access token
**Authorization:** Refresh Token

**Request:**
```json
{
  "refreshToken": "refresh_token"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "token": "new_jwt_token",
    "refreshToken": "new_refresh_token",
    "expiresAt": "2026-08-30T12:00:00Z"
  }
}
```

---

#### POST /api/auth/logout
**Description:** User logout (invalidate tokens)
**Authorization:** Bearer Token

**Response (204):** No content

---

### 5.2 Tenant Management

#### GET /api/tenants/current
**Description:** Get current tenant information
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "code": "DEMO001",
    "name": "Demo Wholesale Co.",
    "businessName": "Demo Wholesale Company Pvt Ltd",
    "email": "info@demo.com",
    "phone": "+911234567890",
    "address": "123 Business St",
    "city": "Mumbai",
    "state": "Maharashtra",
    "pincode": "400001",
    "gstNumber": "27AABCD1234E1Z5",
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

#### PUT /api/tenants/current
**Description:** Update current tenant information
**Authorization:** Bearer Token (Admin only)

**Request:**
```json
{
  "businessName": "Updated Business Name",
  "email": "newemail@demo.com",
  "phone": "+919876543210",
  "address": "456 New Address",
  "city": "Pune",
  "state": "Maharashtra",
  "pincode": "411001",
  "gstNumber": "27AABCD1234E1Z5"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": { /* updated tenant object */ },
  "message": "Tenant updated successfully"
}
```

---

### 5.3 Users

#### GET /api/users
**Description:** Get all users for current tenant
**Authorization:** Bearer Token (Admin, Manager)

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `search` (string, optional)
- `role` (string, optional)
- `isActive` (bool, optional)

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "mobile": "+911234567890",
      "role": "SalesStaff",
      "isActive": true,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5,
  "totalPages": 1
}
```

---

#### POST /api/users
**Description:** Create new user
**Authorization:** Bearer Token (Admin only)

**Request:**
```json
{
  "email": "newuser@example.com",
  "password": "SecureP@ss123",
  "firstName": "Jane",
  "lastName": "Smith",
  "mobile": "+919876543210",
  "role": "SalesStaff"
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "newuser@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "role": "SalesStaff",
    "isActive": true
  },
  "message": "User created successfully"
}
```

---

#### PUT /api/users/{id}
**Description:** Update user
**Authorization:** Bearer Token (Admin only)

**Request:**
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "mobile": "+911111111111",
  "role": "Manager",
  "isActive": true
}
```

**Response (200):** Updated user object

---

#### DELETE /api/users/{id}
**Description:** Deactivate user (soft delete)
**Authorization:** Bearer Token (Admin only)

**Response (204):** No content

---

### 5.4 Products

#### GET /api/products
**Description:** Get all products with pagination and filtering
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `search` (string) - Search by name, SKU, barcode
- `categoryId` (guid, optional)
- `brandId` (guid, optional)
- `isActive` (bool, optional)
- `sortBy` (string, default: name)
- `sortOrder` (asc/desc, default: asc)

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Product A",
      "sku": "SKU001",
      "barcode": "1234567890",
      "categoryId": "guid",
      "categoryName": "Electronics",
      "brandId": "guid",
      "brandName": "Brand X",
      "unitId": "guid",
      "unitName": "Pieces",
      "purchasePrice": 100.00,
      "sellingPrice": 150.00,
      "currentStock": 250,
      "minimumStock": 50,
      "isActive": true,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

---

#### GET /api/products/{id}
**Description:** Get product by ID
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "Product A",
    "sku": "SKU001",
    "barcode": "1234567890",
    "description": "Product description",
    "categoryId": "guid",
    "categoryName": "Electronics",
    "brandId": "guid",
    "brandName": "Brand X",
    "unitId": "guid",
    "unitName": "Pieces",
    "purchasePrice": 100.00,
    "sellingPrice": 150.00,
    "currentStock": 250,
    "minimumStock": 50,
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-08-29T10:00:00Z"
  }
}
```

---

#### POST /api/products
**Description:** Create new product
**Authorization:** Bearer Token (Admin, Manager)

**Request:**
```json
{
  "name": "New Product",
  "sku": "SKU123",
  "barcode": "9876543210",
  "description": "Product description",
  "categoryId": "guid",
  "brandId": "guid",
  "unitId": "guid",
  "purchasePrice": 100.00,
  "sellingPrice": 150.00,
  "minimumStock": 20
}
```

**Response (201):**
```json
{
  "success": true,
  "data": { /* created product object */ },
  "message": "Product created successfully"
}
```

---

#### PUT /api/products/{id}
**Description:** Update product
**Authorization:** Bearer Token (Admin, Manager)

**Request:** Same as POST

**Response (200):** Updated product object

---

#### DELETE /api/products/{id}
**Description:** Deactivate product (soft delete)
**Authorization:** Bearer Token (Admin, Manager)

**Response (204):** No content

---

### 5.5 Categories, Brands, Units

#### GET /api/categories
**Description:** Get all categories
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Electronics",
      "description": "Electronic items",
      "isActive": true
    }
  ]
}
```

#### POST /api/categories
**Request:**
```json
{
  "name": "New Category",
  "description": "Category description"
}
```

#### GET /api/brands
#### POST /api/brands
#### GET /api/units
#### POST /api/units

*(Similar structure to Categories)*

---

### 5.6 Customers

#### GET /api/customers
**Description:** Get all customers with pagination
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `page`, `pageSize`, `search`, `isActive`, `sortBy`, `sortOrder`

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "shopName": "Retail Shop A",
      "ownerName": "Owner Name",
      "mobile": "+911234567890",
      "email": "shop@example.com",
      "address": "123 Shop St",
      "city": "Mumbai",
      "state": "Maharashtra",
      "pincode": "400001",
      "gstNumber": "27XXXXX1234X1Z5",
      "creditLimit": 50000.00,
      "creditDays": 30,
      "outstandingBalance": 15000.00,
      "isActive": true,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 50,
  "totalPages": 3
}
```

---

#### GET /api/customers/{id}
**Description:** Get customer by ID with outstanding balance
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "shopName": "Retail Shop A",
    "ownerName": "Owner Name",
    "mobile": "+911234567890",
    "email": "shop@example.com",
    "address": "123 Shop St",
    "city": "Mumbai",
    "state": "Maharashtra",
    "pincode": "400001",
    "gstNumber": "27XXXXX1234X1Z5",
    "creditLimit": 50000.00,
    "creditDays": 30,
    "openingBalance": 5000.00,
    "outstandingBalance": 15000.00,
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-08-29T10:00:00Z"
  }
}
```

---

#### POST /api/customers
**Description:** Create new customer
**Authorization:** Bearer Token (Admin, Manager, SalesStaff)

**Request:**
```json
{
  "shopName": "New Retail Shop",
  "ownerName": "Shop Owner",
  "mobile": "+919876543210",
  "email": "newshop@example.com",
  "address": "456 Shop Road",
  "city": "Pune",
  "state": "Maharashtra",
  "pincode": "411001",
  "gstNumber": "27XXXXX5678X1Z5",
  "creditLimit": 30000.00,
  "creditDays": 15,
  "openingBalance": 0.00
}
```

**Response (201):** Created customer object

---

#### PUT /api/customers/{id}
**Description:** Update customer
**Authorization:** Bearer Token (Admin, Manager)

**Response (200):** Updated customer object

---

#### GET /api/customers/{id}/ledger
**Description:** Get customer ledger entries
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `fromDate`, `toDate`, `page`, `pageSize`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "customerId": "guid",
    "customerName": "Retail Shop A",
    "openingBalance": 5000.00,
    "entries": [
      {
        "entryDate": "2026-08-01",
        "entryType": "Sale",
        "referenceType": "Sale",
        "referenceId": "guid",
        "referenceNumber": "INV-001",
        "debit": 10000.00,
        "credit": 0.00,
        "runningBalance": 15000.00,
        "notes": "Sale invoice"
      },
      {
        "entryDate": "2026-08-05",
        "entryType": "Payment",
        "referenceType": "Payment",
        "referenceId": "guid",
        "referenceNumber": "PAY-001",
        "debit": 0.00,
        "credit": 5000.00,
        "runningBalance": 10000.00,
        "notes": "Cash payment"
      }
    ],
    "closingBalance": 10000.00,
    "totalDebit": 10000.00,
    "totalCredit": 5000.00
  }
}
```

---

### 5.7 Suppliers

#### GET /api/suppliers
#### GET /api/suppliers/{id}
#### POST /api/suppliers
#### PUT /api/suppliers/{id}
#### DELETE /api/suppliers/{id}

*(Similar structure to Customers, without credit limit and outstanding)*

---

### 5.8 Inventory

#### GET /api/inventory/stock
**Description:** Get current stock for all products
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `page`, `pageSize`, `search`, `categoryId`, `lowStock` (bool)

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "productId": "guid",
      "productName": "Product A",
      "sku": "SKU001",
      "categoryName": "Electronics",
      "unitName": "Pieces",
      "quantityOnHand": 250,
      "reservedQuantity": 0,
      "availableQuantity": 250,
      "minimumStock": 50,
      "isLowStock": false,
      "purchasePrice": 100.00,
      "stockValue": 25000.00,
      "updatedAt": "2026-08-29T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

---

#### GET /api/inventory/stock/{productId}
**Description:** Get stock details for a specific product
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "productId": "guid",
    "productName": "Product A",
    "sku": "SKU001",
    "quantityOnHand": 250,
    "reservedQuantity": 0,
    "availableQuantity": 250,
    "minimumStock": 50,
    "isLowStock": false,
    "updatedAt": "2026-08-29T10:00:00Z"
  }
}
```

---

#### GET /api/inventory/transactions
**Description:** Get inventory transaction history
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `productId` (guid, optional)
- `transactionType` (string, optional)
- `fromDate`, `toDate`
- `page`, `pageSize`

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "productId": "guid",
      "productName": "Product A",
      "transactionType": "Purchase",
      "quantity": 100,
      "referenceType": "Purchase",
      "referenceId": "guid",
      "referenceNumber": "PUR-001",
      "transactionDate": "2026-08-01T10:00:00Z",
      "notes": "Purchase from Supplier A",
      "createdBy": "User Name",
      "createdAt": "2026-08-01T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 500,
  "totalPages": 25
}
```

---

#### POST /api/inventory/opening-stock
**Description:** Add opening stock for products
**Authorization:** Bearer Token (Admin, Manager)

**Request:**
```json
{
  "entries": [
    {
      "productId": "guid",
      "quantity": 100,
      "notes": "Opening stock"
    },
    {
      "productId": "guid",
      "quantity": 200,
      "notes": "Initial inventory"
    }
  ]
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "totalProductsUpdated": 2,
    "totalQuantityAdded": 300
  },
  "message": "Opening stock added successfully"
}
```

---

#### POST /api/inventory/adjustment
**Description:** Adjust stock (correction, damage, etc.)
**Authorization:** Bearer Token (Admin, Manager)

**Request:**
```json
{
  "productId": "guid",
  "quantity": -5,
  "notes": "Damaged items"
}
```

**Response (201):** Adjustment confirmation

---

### 5.9 Purchases

#### GET /api/purchases
**Description:** Get all purchases with pagination
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `page`, `pageSize`, `supplierId`, `fromDate`, `toDate`, `invoiceNumber`

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "supplierId": "guid",
      "supplierName": "Supplier A",
      "invoiceNumber": "PUR-001",
      "purchaseDate": "2026-08-01",
      "subTotal": 9500.00,
      "discount": 500.00,
      "tax": 1000.00,
      "grandTotal": 10000.00,
      "itemsCount": 5,
      "notes": "Purchase note",
      "createdBy": "User Name",
      "createdAt": "2026-08-01T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5
}
```

---

#### GET /api/purchases/{id}
**Description:** Get purchase details with items
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "supplierId": "guid",
    "supplierName": "Supplier A",
    "supplierGST": "27XXXXX1234X1Z5",
    "invoiceNumber": "PUR-001",
    "purchaseDate": "2026-08-01",
    "subTotal": 9500.00,
    "discount": 500.00,
    "tax": 1000.00,
    "grandTotal": 10000.00,
    "notes": "Purchase note",
    "items": [
      {
        "id": "guid",
        "productId": "guid",
        "productName": "Product A",
        "sku": "SKU001",
        "quantity": 100,
        "unitPrice": 95.00,
        "discount": 500.00,
        "tax": 1000.00,
        "total": 10000.00
      }
    ],
    "createdBy": "User Name",
    "createdAt": "2026-08-01T10:00:00Z"
  }
}
```

---

#### POST /api/purchases
**Description:** Create new purchase
**Authorization:** Bearer Token (Admin, Manager)

**Request:**
```json
{
  "supplierId": "guid",
  "invoiceNumber": "PUR-002",
  "purchaseDate": "2026-08-29",
  "notes": "Purchase note",
  "items": [
    {
      "productId": "guid",
      "quantity": 50,
      "unitPrice": 100.00,
      "discount": 100.00,
      "tax": 500.00
    },
    {
      "productId": "guid",
      "quantity": 30,
      "unitPrice": 200.00,
      "discount": 0.00,
      "tax": 600.00
    }
  ]
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "invoiceNumber": "PUR-002",
    "grandTotal": 11600.00,
    "itemsCount": 2
  },
  "message": "Purchase created successfully. Stock updated."
}
```

---

### 5.10 Sales

#### GET /api/sales
**Description:** Get all sales with pagination
**Authorization:** Bearer Token (All Roles - filtered by role)

**Query Parameters:**
- `page`, `pageSize`, `customerId`, `fromDate`, `toDate`, `invoiceNumber`, `paymentStatus`

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "customerId": "guid",
      "customerName": "Retail Shop A",
      "invoiceNumber": "INV-001",
      "saleDate": "2026-08-29",
      "subTotal": 14000.00,
      "discount": 1000.00,
      "tax": 1400.00,
      "grandTotal": 14400.00,
      "paidAmount": 10000.00,
      "dueAmount": 4400.00,
      "paymentStatus": "Partial",
      "itemsCount": 3,
      "createdBy": "Sales Staff",
      "createdAt": "2026-08-29T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 250,
  "totalPages": 13
}
```

---

#### GET /api/sales/{id}
**Description:** Get sale details with items
**Authorization:** Bearer Token (All Roles)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "customerId": "guid",
    "customerName": "Retail Shop A",
    "customerShopName": "Retail Shop A",
    "customerMobile": "+911234567890",
    "customerAddress": "123 Shop St, Mumbai",
    "customerGST": "27XXXXX1234X1Z5",
    "invoiceNumber": "INV-001",
    "saleDate": "2026-08-29",
    "subTotal": 14000.00,
    "discount": 1000.00,
    "tax": 1400.00,
    "grandTotal": 14400.00,
    "paidAmount": 10000.00,
    "dueAmount": 4400.00,
    "paymentStatus": "Partial",
    "notes": "Sale note",
    "items": [
      {
        "id": "guid",
        "productId": "guid",
        "productName": "Product A",
        "sku": "SKU001",
        "quantity": 100,
        "unitPrice": 150.00,
        "discount": 1000.00,
        "tax": 1400.00,
        "total": 14400.00
      }
    ],
    "createdBy": "Sales Staff",
    "createdAt": "2026-08-29T10:00:00Z"
  }
}
```

---

#### POST /api/sales
**Description:** Create new sale (invoice)
**Authorization:** Bearer Token (Admin, Manager, SalesStaff)

**Request:**
```json
{
  "customerId": "guid",
  "invoiceNumber": "INV-002",
  "saleDate": "2026-08-29",
  "paidAmount": 5000.00,
  "notes": "Sale note",
  "items": [
    {
      "productId": "guid",
      "quantity": 10,
      "unitPrice": 150.00,
      "discount": 0.00,
      "tax": 150.00
    },
    {
      "productId": "guid",
      "quantity": 5,
      "unitPrice": 200.00,
      "discount": 0.00,
      "tax": 100.00
    }
  ]
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "invoiceNumber": "INV-002",
    "grandTotal": 2750.00,
    "paidAmount": 2750.00,
    "dueAmount": 0.00,
    "paymentStatus": "Paid"
  },
  "message": "Sale created successfully. Stock and ledger updated."
}
```

**Business Rules Validated:**
- Stock availability for each product
- Customer credit limit (if configured)
- Payment amount <= Grand Total

---

#### GET /api/sales/{id}/invoice
**Description:** Get printable invoice (HTML/PDF)
**Authorization:** Bearer Token (All Roles)

**Response (200):** HTML or PDF invoice

---

### 5.11 Payments

#### GET /api/payments
**Description:** Get all payments
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `customerId`, `fromDate`, `toDate`, `paymentMethod`, `page`, `pageSize`

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "customerId": "guid",
      "customerName": "Retail Shop A",
      "paymentDate": "2026-08-29",
      "amount": 5000.00,
      "paymentMethod": "Cash",
      "referenceNumber": null,
      "notes": "Cash payment",
      "createdBy": "User Name",
      "createdAt": "2026-08-29T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

---

#### POST /api/payments
**Description:** Record payment from customer
**Authorization:** Bearer Token (Admin, Manager, SalesStaff)

**Request:**
```json
{
  "customerId": "guid",
  "paymentDate": "2026-08-29",
  "amount": 5000.00,
  "paymentMethod": "BankTransfer",
  "referenceNumber": "TXN123456789",
  "notes": "Bank transfer payment"
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "amount": 5000.00,
    "newOutstandingBalance": 5400.00
  },
  "message": "Payment recorded successfully. Customer outstanding updated."
}
```

---

### 5.12 Reports

#### GET /api/reports/dashboard
**Description:** Dashboard summary metrics
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `fromDate`, `toDate` (optional, defaults to current month)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "todaySales": 25000.00,
    "todaySalesCount": 15,
    "monthSales": 150000.00,
    "monthSalesCount": 200,
    "totalOutstanding": 50000.00,
    "lowStockCount": 12,
    "totalProducts": 500,
    "totalActiveCustomers": 85,
    "recentSales": [
      {
        "id": "guid",
        "invoiceNumber": "INV-001",
        "customerName": "Shop A",
        "saleDate": "2026-08-29",
        "grandTotal": 5000.00,
        "paymentStatus": "Paid"
      }
    ],
    "topCustomers": [
      {
        "customerId": "guid",
        "customerName": "Shop A",
        "totalSales": 50000.00,
        "salesCount": 25
      }
    ],
    "topProducts": [
      {
        "productId": "guid",
        "productName": "Product A",
        "totalQuantitySold": 500,
        "totalSalesAmount": 75000.00
      }
    ]
  }
}
```

---

#### GET /api/reports/sales/summary
**Description:** Sales summary report
**Authorization:** Bearer Token (All Roles)

**Query Parameters:**
- `fromDate`, `toDate` (required)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "fromDate": "2026-08-01",
    "toDate": "2026-08-31",
    "totalSales": 150000.00,
    "totalInvoices": 200,
    "totalItemsSold": 1500,
    "totalDiscount": 5000.00,
    "totalTax": 15000.00,
    "averageOrderValue": 750.00,
    "paidAmount": 120000.00,
    "outstandingAmount": 30000.00
  }
}
```

---

#### GET /api/reports/sales/by-product
**Description:** Sales breakdown by product
**Query Parameters:** `fromDate`, `toDate`, `page`, `pageSize`

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "productId": "guid",
      "productName": "Product A",
      "sku": "SKU001",
      "totalQuantitySold": 500,
      "totalSalesAmount": 75000.00,
      "averageSellingPrice": 150.00
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150
}
```

---

#### GET /api/reports/sales/by-customer
**Description:** Sales breakdown by customer

---

#### GET /api/reports/purchases/summary
**Description:** Purchases summary report

---

#### GET /api/reports/inventory/current-stock
**Description:** Current stock report (same as /api/inventory/stock)

---

#### GET /api/reports/inventory/low-stock
**Description:** Low stock products report

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "productId": "guid",
      "productName": "Product B",
      "sku": "SKU002",
      "quantityOnHand": 15,
      "minimumStock": 50,
      "shortfall": 35,
      "suggestedReorder": 100
    }
  ]
}
```

---

#### GET /api/reports/customers/outstanding
**Description:** Customer outstanding report

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "customerId": "guid",
      "customerName": "Shop A",
      "outstandingBalance": 15000.00,
      "creditLimit": 50000.00,
      "creditDays": 30,
      "lastPaymentDate": "2026-08-25",
      "overdueAmount": 5000.00
    }
  ],
  "totalOutstanding": 150000.00
}
```

---

### 5.13 Imports

#### POST /api/imports/products
**Description:** Import products from Excel
**Authorization:** Bearer Token (Admin, Manager)

**Request (multipart/form-data):**
```
file: products.xlsx
```

**Response (202):**
```json
{
  "success": true,
  "data": {
    "importId": "guid",
    "status": "Processing"
  },
  "message": "Import started. Check status with GET /api/imports/{importId}"
}
```

---

#### GET /api/imports/{id}/status
**Description:** Get import status and results
**Authorization:** Bearer Token (Admin, Manager)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "importId": "guid",
    "entityType": "Products",
    "status": "Completed",
    "totalRows": 100,
    "successfulRows": 95,
    "skippedRows": 3,
    "errorRows": 2,
    "errors": [
      {
        "rowNumber": 5,
        "error": "Duplicate SKU: SKU001"
      },
      {
        "rowNumber": 12,
        "error": "Invalid price: must be greater than zero"
      }
    ],
    "completedAt": "2026-08-29T10:05:00Z"
  }
}
```

---

#### GET /api/imports/templates/products
**Description:** Download Excel template for product import
**Authorization:** Bearer Token (Admin, Manager)

**Response (200):** Excel file download

---

#### POST /api/imports/customers
**Description:** Import customers from Excel

#### POST /api/imports/suppliers
**Description:** Import suppliers from Excel

#### POST /api/imports/opening-stock
**Description:** Import opening stock from Excel

---

## 6. Request/Response Examples

### 6.1 Creating a Complete Sale Flow

#### Step 1: Search for Customer
```http
GET /api/customers?search=Shop+A
Authorization: Bearer {token}
```

#### Step 2: Search for Products
```http
GET /api/products?search=Product+A
Authorization: Bearer {token}
```

#### Step 3: Check Stock Availability
```http
GET /api/inventory/stock/{productId}
Authorization: Bearer {token}
```

#### Step 4: Create Sale
```http
POST /api/sales
Authorization: Bearer {token}
Content-Type: application/json

{
  "customerId": "customer-guid",
  "invoiceNumber": "INV-123",
  "saleDate": "2026-08-29",
  "paidAmount": 5000.00,
  "items": [
    {
      "productId": "product-guid",
      "quantity": 10,
      "unitPrice": 150.00,
      "discount": 0.00,
      "tax": 150.00
    }
  ]
}
```

#### Step 5: Get Invoice
```http
GET /api/sales/{saleId}/invoice
Authorization: Bearer {token}
```

---

### 6.2 Recording Payment Flow

#### Step 1: Get Customer Outstanding
```http
GET /api/customers/{customerId}
Authorization: Bearer {token}
```

#### Step 2: Record Payment
```http
POST /api/payments
Authorization: Bearer {token}
Content-Type: application/json

{
  "customerId": "customer-guid",
  "paymentDate": "2026-08-29",
  "amount": 5000.00,
  "paymentMethod": "Cash",
  "notes": "Cash payment"
}
```

#### Step 3: Verify Updated Balance
```http
GET /api/customers/{customerId}
Authorization: Bearer {token}
```

---

## Appendix A: Rate Limiting

**Limits:**
- Anonymous: 10 requests/minute
- Authenticated: 1000 requests/hour
- Imports: 10 uploads/hour

**Headers:**
```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1672531200
```

---

## Appendix B: Webhook Support (Future)

For future real-time integrations:
- `sale.created`
- `payment.received`
- `stock.low`
- `customer.outstanding.exceeded`

---

**Document Status:** This is a living document and will be updated as APIs evolve during implementation.
