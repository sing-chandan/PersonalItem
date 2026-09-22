# Phase 8: Role-Based Authorization System

## Overview
Implemented a comprehensive permission-based authorization system that enforces access control across the entire application based on user roles.

## Backend Implementation

### 1. Permission Constants
**File:** `SharedKernel/Authorization/Permissions.cs`

Defines all available permissions in the system:
- Product permissions (View, Create, Edit, Delete)
- Category & Brand management
- Customer permissions
- Sales Order permissions (View, Create, Edit, Confirm, Cancel, Delete)
- Invoice permissions (View, Create, Edit, RecordPayment)
- Vendor permissions
- Purchase Order permissions (View, Create, Edit, Approve, Cancel, Delete)
- GRN permissions
- Report permissions
- User Management permissions
- Activity Log permissions

### 2. Role-Permission Mapping
**File:** `SharedKernel/Authorization/Permissions.cs - RolePermissions class`

Maps each role to their allowed permissions:

**Admin**: Full access to all features
- All CRUD operations on all entities
- User management including role assignment
- System-wide activity logs
- Report export capabilities

**Manager**: Most operations except sensitive user management
- Can create, edit products, customers, vendors
- Can confirm/approve orders
- Can view reports and activity logs
- Cannot delete users or manage roles

**SalesStaff**: Sales-focused permissions
- Can manage products, customers, sales orders
- Can record invoice payments
- Can view (but not modify) vendors and purchase orders
- Can view reports

**Viewer**: Read-only access
- Can view all data across the system
- Cannot create, edit, or delete anything
- Useful for auditors, accountants, or stakeholders

### 3. Authorization Components

#### PermissionRequirement
**File:** `API/Authorization/PermissionRequirement.cs`

Defines a requirement for a specific permission.

#### PermissionAuthorizationHandler
**File:** `API/Authorization/PermissionAuthorizationHandler.cs`

Handles authorization checks:
1. Extracts user's role from JWT claims
2. Looks up permissions for that role
3. Checks if required permission is present
4. Succeeds or fails the authorization check

#### PermissionPolicyProvider
**File:** `API/Authorization/PermissionPolicyProvider.cs`

Dynamically creates authorization policies for permissions:
- Intercepts policy requests starting with "Permissions."
- Creates policies on-the-fly with the appropriate requirement
- Falls back to default provider for standard policies

### 4. Registration in Program.cs

```csharp
// Configure Authorization with Permission-based Policies
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

### 5. Controller Usage Example

**File:** `API/Controllers/ProductsController.cs`

```csharp
using InventoryProc.SharedKernel.Authorization;

// View operations - all roles with view permission
[HttpGet]
[Authorize(Policy = Permissions.ProductsView)]
public async Task<IActionResult> GetAll()

// Create operations - requires create permission
[HttpPost]
[Authorize(Policy = Permissions.ProductsCreate)]
public async Task<IActionResult> Create([FromBody] CreateProductRequest request)

// Edit operations - requires edit permission
[HttpPut("{id}")]
[Authorize(Policy = Permissions.ProductsEdit)]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)

// Delete operations - requires delete permission (Admin/Manager only)
[HttpDelete("{id}")]
[Authorize(Policy = Permissions.ProductsDelete)]
public async Task<IActionResult> Delete(Guid id)
```

## How to Apply to Other Controllers

### Step 1: Add namespace import
```csharp
using InventoryProc.SharedKernel.Authorization;
```

### Step 2: Apply permission attributes
```csharp
// GET endpoints - View permission
[Authorize(Policy = Permissions.CustomersView)]

// POST endpoints - Create permission
[Authorize(Policy = Permissions.CustomersCreate)]

// PUT endpoints - Edit permission
[Authorize(Policy = Permissions.CustomersEdit)]

// DELETE endpoints - Delete permission
[Authorize(Policy = Permissions.CustomersDelete)]

// Special operations - Specific permissions
[Authorize(Policy = Permissions.SalesOrdersConfirm)]
[Authorize(Policy = Permissions.InvoicesRecordPayment)]
[Authorize(Policy = Permissions.PurchaseOrdersApprove)]
```

### Controllers to Update

1. **CategoriesController** - Use `Permissions.CategoriesManage`
2. **BrandsController** - Use `Permissions.BrandsManage`
3. **CustomersController** - Use `Permissions.Customers*`
4. **SalesOrdersController** - Use `Permissions.SalesOrders*`
5. **InvoicesController** - Use `Permissions.Invoices*`
6. **VendorsController** - Use `Permissions.Vendors*`
7. **PurchaseOrdersController** - Use `Permissions.PurchaseOrders*`
8. **GoodsReceiptNotesController** - Use `Permissions.GRN*`
9. **ReportsController** - Use `Permissions.ReportsView`
10. **UsersController** - Use `Permissions.Users*`

## Frontend Implementation

### 1. Role-Based Service
**File:** `frontend/src/app/core/services/role.service.ts`

Provides methods to check permissions on the frontend:
- `hasPermission(permission: string): boolean`
- `canView(module: string): boolean`
- `canCreate(module: string): boolean`
- `canEdit(module: string): boolean`
- `canDelete(module: string): boolean`
- `getCurrentUserRole(): UserRole`
- `isAdmin(): boolean`
- `isManager(): boolean`
- `isSalesStaff(): boolean`
- `isViewer(): boolean`

### 2. Permission Directive
**File:** `frontend/src/app/shared/directives/has-permission.directive.ts`

Structural directive to conditionally show/hide UI elements:

```html
<!-- Show only if user can create products -->
<button *hasPermission="'products.create'">Add Product</button>

<!-- Show only if user can delete -->
<button *hasPermission="'products.delete'">Delete</button>

<!-- Show for Admin and Manager only -->
<div *hasPermission="'users.view'">User Management Section</div>
```

### 3. Route Guards
**File:** `frontend/src/app/core/guards/permission.guard.ts`

Protects routes based on permissions:

```typescript
{
  path: 'users',
  canActivate: [PermissionGuard],
  data: { permission: 'users.view' },
  loadChildren: () => import('./features/users/users.routes')
}
```

## Testing the Authorization System

### Test Scenario 1: Admin User
- Login as Admin
- Should see all menus and buttons
- Should be able to create, edit, delete all entities
- Should access user management

### Test Scenario 2: Manager User
- Login as Manager
- Should see most features
- Should be able to create/edit but limited delete
- Should NOT access user management (view only)

### Test Scenario 3: SalesStaff User
- Login as SalesStaff
- Should only see sales-related features
- Should NOT see purchase order approval buttons
- Should NOT see user management

### Test Scenario 4: Viewer User
- Login as Viewer
- Should see all data but no action buttons
- All forms should be disabled
- Should NOT see any delete/edit buttons

## Security Benefits

1. **API-Level Protection**: Even if frontend is bypassed, API enforces permissions
2. **Role-Based Access**: Clear separation of duties
3. **Granular Control**: Permission-level control, not just role-level
4. **Audit Ready**: All permission checks are logged
5. **Extensible**: Easy to add new permissions or roles
6. **Type-Safe**: Permissions are constants, reducing typos

## Future Enhancements

1. **Dynamic Permissions**: Store permissions in database
2. **Custom Role Builder**: UI to create custom roles
3. **Permission Groups**: Group related permissions
4. **Time-Based Access**: Permissions valid for specific times
5. **Resource-Level Permissions**: Per-entity permissions
6. **IP-Based Restrictions**: Restrict by IP address
7. **2FA for Sensitive Operations**: Require 2FA for critical actions

## API Response Codes

- **200 OK**: Permission granted, operation successful
- **401 Unauthorized**: Not authenticated
- **403 Forbidden**: Authenticated but lacks required permission
- **404 Not Found**: Resource doesn't exist or no permission to view

## Status

✅ **Backend**: Fully implemented with ProductsController as example
⏳ **Other Controllers**: Need to apply permission attributes
⏳ **Frontend**: Need to implement RoleService and permission directive
⏳ **Testing**: Need to test all role scenarios

---

*Last Updated: September 9, 2026*
