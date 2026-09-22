import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

export enum UserRole {
  Admin = 1,
  Manager = 2,
  SalesStaff = 3,
  Viewer = 4
}

export enum Permission {
  // Products
  ProductsView = 'products.view',
  ProductsCreate = 'products.create',
  ProductsEdit = 'products.edit',
  ProductsDelete = 'products.delete',

  // Categories & Brands
  CategoriesManage = 'categories.manage',
  BrandsManage = 'brands.manage',

  // Customers
  CustomersView = 'customers.view',
  CustomersCreate = 'customers.create',
  CustomersEdit = 'customers.edit',
  CustomersDelete = 'customers.delete',

  // Sales Orders
  SalesOrdersView = 'salesOrders.view',
  SalesOrdersCreate = 'salesOrders.create',
  SalesOrdersEdit = 'salesOrders.edit',
  SalesOrdersConfirm = 'salesOrders.confirm',
  SalesOrdersCancel = 'salesOrders.cancel',
  SalesOrdersDelete = 'salesOrders.delete',

  // Invoices
  InvoicesView = 'invoices.view',
  InvoicesCreate = 'invoices.create',
  InvoicesEdit = 'invoices.edit',
  InvoicesRecordPayment = 'invoices.recordPayment',

  // Vendors
  VendorsView = 'vendors.view',
  VendorsCreate = 'vendors.create',
  VendorsEdit = 'vendors.edit',
  VendorsDelete = 'vendors.delete',

  // Purchase Orders
  PurchaseOrdersView = 'purchaseOrders.view',
  PurchaseOrdersCreate = 'purchaseOrders.create',
  PurchaseOrdersEdit = 'purchaseOrders.edit',
  PurchaseOrdersApprove = 'purchaseOrders.approve',
  PurchaseOrdersCancel = 'purchaseOrders.cancel',
  PurchaseOrdersDelete = 'purchaseOrders.delete',

  // GRN
  GRNView = 'grn.view',
  GRNCreate = 'grn.create',

  // Reports
  ReportsView = 'reports.view',
  ReportsExport = 'reports.export',

  // Users
  UsersView = 'users.view',
  UsersCreate = 'users.create',
  UsersEdit = 'users.edit',
  UsersDelete = 'users.delete',
  UsersManageRoles = 'users.manageRoles',

  // Activity Logs
  ActivityLogsView = 'activityLogs.view'
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private rolePermissions: Map<UserRole, Permission[]> = new Map([
    [UserRole.Admin, [
      // Admin has all permissions
      Permission.ProductsView, Permission.ProductsCreate, Permission.ProductsEdit, Permission.ProductsDelete,
      Permission.CategoriesManage, Permission.BrandsManage,
      Permission.CustomersView, Permission.CustomersCreate, Permission.CustomersEdit, Permission.CustomersDelete,
      Permission.SalesOrdersView, Permission.SalesOrdersCreate, Permission.SalesOrdersEdit,
      Permission.SalesOrdersConfirm, Permission.SalesOrdersCancel, Permission.SalesOrdersDelete,
      Permission.InvoicesView, Permission.InvoicesCreate, Permission.InvoicesEdit, Permission.InvoicesRecordPayment,
      Permission.VendorsView, Permission.VendorsCreate, Permission.VendorsEdit, Permission.VendorsDelete,
      Permission.PurchaseOrdersView, Permission.PurchaseOrdersCreate, Permission.PurchaseOrdersEdit,
      Permission.PurchaseOrdersApprove, Permission.PurchaseOrdersCancel, Permission.PurchaseOrdersDelete,
      Permission.GRNView, Permission.GRNCreate,
      Permission.ReportsView, Permission.ReportsExport,
      Permission.UsersView, Permission.UsersCreate, Permission.UsersEdit, Permission.UsersDelete, Permission.UsersManageRoles,
      Permission.ActivityLogsView
    ]],
    [UserRole.Manager, [
      // Manager can do most operations but not delete users
      Permission.ProductsView, Permission.ProductsCreate, Permission.ProductsEdit,
      Permission.CategoriesManage, Permission.BrandsManage,
      Permission.CustomersView, Permission.CustomersCreate, Permission.CustomersEdit,
      Permission.SalesOrdersView, Permission.SalesOrdersCreate, Permission.SalesOrdersEdit,
      Permission.SalesOrdersConfirm, Permission.SalesOrdersCancel,
      Permission.InvoicesView, Permission.InvoicesCreate, Permission.InvoicesEdit, Permission.InvoicesRecordPayment,
      Permission.VendorsView, Permission.VendorsCreate, Permission.VendorsEdit,
      Permission.PurchaseOrdersView, Permission.PurchaseOrdersCreate, Permission.PurchaseOrdersEdit,
      Permission.PurchaseOrdersApprove, Permission.PurchaseOrdersCancel,
      Permission.GRNView, Permission.GRNCreate,
      Permission.ReportsView, Permission.ReportsExport,
      Permission.UsersView,
      Permission.ActivityLogsView
    ]],
    [UserRole.SalesStaff, [
      // Sales staff - sales focused
      Permission.ProductsView, Permission.ProductsCreate, Permission.ProductsEdit,
      Permission.CustomersView, Permission.CustomersCreate, Permission.CustomersEdit,
      Permission.SalesOrdersView, Permission.SalesOrdersCreate, Permission.SalesOrdersEdit,
      Permission.InvoicesView, Permission.InvoicesRecordPayment,
      Permission.VendorsView,
      Permission.PurchaseOrdersView,
      Permission.GRNView,
      Permission.ReportsView
    ]],
    [UserRole.Viewer, [
      // Viewer - read-only access
      Permission.ProductsView,
      Permission.CustomersView,
      Permission.SalesOrdersView,
      Permission.InvoicesView,
      Permission.VendorsView,
      Permission.PurchaseOrdersView,
      Permission.GRNView,
      Permission.ReportsView
    ]]
  ]);

  constructor(private authService: AuthService) {}

  /**
   * Get current user's role from auth service
   */
  getCurrentUserRole(): UserRole | null {
    const currentUser = this.authService.currentUser;
    if (!currentUser || !currentUser.role) {
      return null;
    }

    // Map role string to UserRole enum
    switch (currentUser.role.toLowerCase()) {
      case 'admin': return UserRole.Admin;
      case 'manager': return UserRole.Manager;
      case 'salesstaff': return UserRole.SalesStaff;
      case 'viewer': return UserRole.Viewer;
      default: return null;
    }
  }

  /**
   * Check if current user has a specific permission
   */
  hasPermission(permission: Permission): boolean {
    const role = this.getCurrentUserRole();
    if (role === null) {
      return false;
    }

    const permissions = this.rolePermissions.get(role);
    return permissions ? permissions.includes(permission) : false;
  }

  /**
   * Check multiple permissions (user must have ALL)
   */
  hasAllPermissions(permissions: Permission[]): boolean {
    return permissions.every(p => this.hasPermission(p));
  }

  /**
   * Check multiple permissions (user must have AT LEAST ONE)
   */
  hasAnyPermission(permissions: Permission[]): boolean {
    return permissions.some(p => this.hasPermission(p));
  }

  /**
   * Role check helpers
   */
  isAdmin(): boolean {
    return this.getCurrentUserRole() === UserRole.Admin;
  }

  isManager(): boolean {
    return this.getCurrentUserRole() === UserRole.Manager;
  }

  isSalesStaff(): boolean {
    return this.getCurrentUserRole() === UserRole.SalesStaff;
  }

  isViewer(): boolean {
    return this.getCurrentUserRole() === UserRole.Viewer;
  }

  /**
   * Check if user can perform common operations on a module
   */
  canView(module: 'products' | 'customers' | 'salesOrders' | 'invoices' | 'vendors' | 'purchaseOrders' | 'grn' | 'reports' | 'users'): boolean {
    const permissionMap: { [key: string]: Permission } = {
      'products': Permission.ProductsView,
      'customers': Permission.CustomersView,
      'salesOrders': Permission.SalesOrdersView,
      'invoices': Permission.InvoicesView,
      'vendors': Permission.VendorsView,
      'purchaseOrders': Permission.PurchaseOrdersView,
      'grn': Permission.GRNView,
      'reports': Permission.ReportsView,
      'users': Permission.UsersView
    };
    return this.hasPermission(permissionMap[module]);
  }

  canCreate(module: 'products' | 'customers' | 'salesOrders' | 'invoices' | 'vendors' | 'purchaseOrders' | 'grn' | 'users'): boolean {
    const permissionMap: { [key: string]: Permission } = {
      'products': Permission.ProductsCreate,
      'customers': Permission.CustomersCreate,
      'salesOrders': Permission.SalesOrdersCreate,
      'invoices': Permission.InvoicesCreate,
      'vendors': Permission.VendorsCreate,
      'purchaseOrders': Permission.PurchaseOrdersCreate,
      'grn': Permission.GRNCreate,
      'users': Permission.UsersCreate
    };
    return this.hasPermission(permissionMap[module]);
  }

  canEdit(module: 'products' | 'customers' | 'salesOrders' | 'invoices' | 'vendors' | 'purchaseOrders' | 'users'): boolean {
    const permissionMap: { [key: string]: Permission } = {
      'products': Permission.ProductsEdit,
      'customers': Permission.CustomersEdit,
      'salesOrders': Permission.SalesOrdersEdit,
      'invoices': Permission.InvoicesEdit,
      'vendors': Permission.VendorsEdit,
      'purchaseOrders': Permission.PurchaseOrdersEdit,
      'users': Permission.UsersEdit
    };
    return this.hasPermission(permissionMap[module]);
  }

  canDelete(module: 'products' | 'customers' | 'salesOrders' | 'vendors' | 'purchaseOrders' | 'users'): boolean {
    const permissionMap: { [key: string]: Permission } = {
      'products': Permission.ProductsDelete,
      'customers': Permission.CustomersDelete,
      'salesOrders': Permission.SalesOrdersDelete,
      'vendors': Permission.VendorsDelete,
      'purchaseOrders': Permission.PurchaseOrdersDelete,
      'users': Permission.UsersDelete
    };
    return this.hasPermission(permissionMap[module]);
  }
}
