# Phase 8: Authorization - Usage Guide

## Quick Start

### Backend: Protecting API Endpoints

1. **Add namespace import:**
```csharp
using InventoryProc.SharedKernel.Authorization;
```

2. **Apply permission attributes to controller methods:**

```csharp
// View permissions (all roles can access based on their view rights)
[HttpGet]
[Authorize(Policy = Permissions.ProductsView)]
public async Task<IActionResult> GetAll()

// Create permissions (only roles with create rights)
[HttpPost]
[Authorize(Policy = Permissions.ProductsCreate)]
public async Task<IActionResult> Create([FromBody] CreateProductRequest request)

// Edit permissions (only roles with edit rights)
[HttpPut("{id}")]
[Authorize(Policy = Permissions.ProductsEdit)]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)

// Delete permissions (only Admin and Manager)
[HttpDelete("{id}")]
[Authorize(Policy = Permissions.ProductsDelete)]
public async Task<IActionResult> Delete(Guid id)

// Special operation permissions
[HttpPost("{id}/confirm")]
[Authorize(Policy = Permissions.SalesOrdersConfirm)]
public async Task<IActionResult> Confirm(Guid id)
```

### Frontend: Role-Based UI

#### 1. Using the Role Service in Components

```typescript
import { Component, OnInit } from '@angular/core';
import { RoleService, Permission } from '../../core/services/role.service';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  canCreateProducts = false;
  canEditProducts = false;
  canDeleteProducts = false;
  isAdmin = false;

  constructor(private roleService: RoleService) {}

  ngOnInit(): void {
    // Check specific permissions
    this.canCreateProducts = this.roleService.hasPermission(Permission.ProductsCreate);
    this.canEditProducts = this.roleService.hasPermission(Permission.ProductsEdit);
    this.canDeleteProducts = this.roleService.hasPermission(Permission.ProductsDelete);

    // Check role
    this.isAdmin = this.roleService.isAdmin();

    // Or use helper methods
    this.canCreateProducts = this.roleService.canCreate('products');
    this.canEditProducts = this.roleService.canEdit('products');
    this.canDeleteProducts = this.roleService.canDelete('products');
  }

  onDelete(id: string): void {
    if (!this.canDeleteProducts) {
      alert('You do not have permission to delete products');
      return;
    }
    // Proceed with delete
  }
}
```

#### 2. Using the Permission Directive in Templates

```html
<!-- Show "Add Product" button only for users with create permission -->
<button
  *hasPermission="Permission.ProductsCreate"
  class="btn btn-primary"
  (click)="onAddProduct()">
  Add Product
</button>

<!-- Show edit button only for users with edit permission -->
<button
  *hasPermission="Permission.ProductsEdit"
  class="btn btn-warning btn-sm"
  (click)="onEdit(product.id)">
  Edit
</button>

<!-- Show delete button only for Admin and Manager -->
<button
  *hasPermission="Permission.ProductsDelete"
  class="btn btn-danger btn-sm"
  (click)="onDelete(product.id)">
  Delete
</button>

<!-- Show entire section only for specific permission -->
<div *hasPermission="Permission.UsersView" class="card">
  <div class="card-body">
    <h5>User Management</h5>
    <!-- User management content -->
  </div>
</div>
```

#### 3. Conditional Logic Using ngIf

```html
<!-- Alternative to directive using component properties -->
<button
  *ngIf="canCreateProducts"
  class="btn btn-primary"
  (click)="onAddProduct()">
  Add Product
</button>

<div *ngIf="isAdmin" class="admin-panel">
  <!-- Admin-only content -->
</div>
```

#### 4. Disable Instead of Hide

```html
<!-- Disable button but keep visible -->
<button
  class="btn btn-primary"
  [disabled]="!canCreateProducts"
  (click)="onAddProduct()">
  Add Product
</button>

<!-- Show tooltip explaining why disabled -->
<button
  class="btn btn-danger"
  [disabled]="!canDeleteProducts"
  [title]="!canDeleteProducts ? 'You do not have permission to delete' : 'Delete item'"
  (click)="onDelete(product.id)">
  Delete
</button>
```

## Permission Reference

### Products
- `Permissions.ProductsView` - View products
- `Permissions.ProductsCreate` - Create new products
- `Permissions.ProductsEdit` - Edit existing products
- `Permissions.ProductsDelete` - Delete products (Admin only)

### Customers
- `Permissions.CustomersView`
- `Permissions.CustomersCreate`
- `Permissions.CustomersEdit`
- `Permissions.CustomersDelete`

### Sales Orders
- `Permissions.SalesOrdersView`
- `Permissions.SalesOrdersCreate`
- `Permissions.SalesOrdersEdit`
- `Permissions.SalesOrdersConfirm` - Confirm/approve orders
- `Permissions.SalesOrdersCancel` - Cancel orders
- `Permissions.SalesOrdersDelete`

### Invoices
- `Permissions.InvoicesView`
- `Permissions.InvoicesCreate`
- `Permissions.InvoicesEdit`
- `Permissions.InvoicesRecordPayment` - Record payments

### Vendors
- `Permissions.VendorsView`
- `Permissions.VendorsCreate`
- `Permissions.VendorsEdit`
- `Permissions.VendorsDelete`

### Purchase Orders
- `Permissions.PurchaseOrdersView`
- `Permissions.PurchaseOrdersCreate`
- `Permissions.PurchaseOrdersEdit`
- `Permissions.PurchaseOrdersApprove` - Approve POs (Manager+)
- `Permissions.PurchaseOrdersCancel`
- `Permissions.PurchaseOrdersDelete`

### Reports
- `Permissions.ReportsView` - View all reports
- `Permissions.ReportsExport` - Export reports to Excel/PDF

### Users & Activity
- `Permissions.UsersView` - View user list
- `Permissions.UsersCreate` - Create new users
- `Permissions.UsersEdit` - Edit user details
- `Permissions.UsersDelete` - Delete users (Admin only)
- `Permissions.UsersManageRoles` - Change user roles (Admin only)
- `Permissions.ActivityLogsView` - View activity logs

## Role Capabilities Summary

### Admin (Full Access)
✅ All permissions
✅ User management including role assignment
✅ Delete operations
✅ Activity logs

### Manager (Most Operations)
✅ Create, Edit (all modules)
✅ Approve orders
✅ View activity logs
✅ View users
❌ Delete users
❌ Manage roles

### SalesStaff (Sales Focus)
✅ Manage products, customers, sales orders
✅ Record payments
✅ View vendors and purchase orders
✅ View reports
❌ Approve purchase orders
❌ User management
❌ Delete operations

### Viewer (Read-Only)
✅ View all data
❌ Create, edit, or delete anything
❌ No workflow actions
❌ No user management

## Testing Examples

### Test Case 1: Viewer Cannot Create
1. Login as Viewer
2. Navigate to Products page
3. "Add Product" button should NOT be visible
4. Try to call POST /api/products directly
5. Should receive 403 Forbidden

### Test Case 2: SalesStaff Cannot Approve PO
1. Login as SalesStaff
2. Navigate to Purchase Orders
3. Can view orders but "Approve" button should NOT be visible
4. Try to call POST /api/purchase-orders/{id}/approve
5. Should receive 403 Forbidden

### Test Case 3: Manager Cannot Delete Users
1. Login as Manager
2. Navigate to Users page
3. Can see user list
4. "Delete" button should NOT be visible on user rows
5. Try to call DELETE /api/users/{id}
6. Should receive 403 Forbidden

### Test Case 4: Admin Has Full Access
1. Login as Admin
2. All buttons and menu items visible
3. All API calls succeed
4. Can perform any operation

## Frontend Component Import

Remember to import Permission enum and RoleService:

```typescript
import { RoleService, Permission } from '../../core/services/role.service';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';

@Component({
  // ...
  imports: [
    CommonModule,
    HasPermissionDirective // Add this for *hasPermission directive
  ]
})
```

## Common Patterns

### Pattern 1: Action Button Row
```html
<div class="btn-group btn-group-sm">
  <button
    *hasPermission="Permission.ProductsView"
    class="btn btn-info"
    (click)="onView(product.id)">
    View
  </button>
  <button
    *hasPermission="Permission.ProductsEdit"
    class="btn btn-warning"
    (click)="onEdit(product.id)">
    Edit
  </button>
  <button
    *hasPermission="Permission.ProductsDelete"
    class="btn btn-danger"
    (click)="onDelete(product.id)">
    Delete
  </button>
</div>
```

### Pattern 2: Conditional Menu Items
```html
<li class="nav-item" *hasPermission="Permission.UsersView">
  <a class="nav-link" routerLink="/users">
    <i class="icon-users"></i> Users
  </a>
</li>
```

### Pattern 3: Form Submit Button
```html
<form [formGroup]="productForm" (ngSubmit)="onSubmit()">
  <!-- Form fields -->

  <button
    type="submit"
    class="btn btn-primary"
    [disabled]="productForm.invalid || !canSaveProduct">
    {{ isEditMode ? 'Update' : 'Create' }}
  </button>
</form>
```

---

**Phase 8 Status:** ✅ Backend Complete | ✅ Frontend Services Complete | ⏳ Apply to All Controllers & Components

*Last Updated: September 9, 2026*
