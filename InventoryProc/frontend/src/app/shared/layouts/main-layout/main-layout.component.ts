import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss'],
})
export class MainLayoutComponent {
  sidebarCollapsed = false;
  currentUser: any = null;

  menuItems = [
    {
      label: 'Dashboard',
      icon: '📊',
      route: '/dashboard',
      expanded: false,
    },
    {
      label: 'Products',
      icon: '📦',
      route: '/products',
      expanded: false,
      children: [
        { label: 'All Products', icon: '📋', route: '/products/list' },
        { label: 'Add Product', icon: '➕', route: '/products/create' },
        { label: 'Categories', icon: '📁', route: '/products/categories' },
        { label: 'Brands', icon: '🏷️', route: '/products/brands' },
      ],
    },
    {
      label: 'Sales',
      icon: '💰',
      route: '/sales',
      expanded: false,
      children: [
        { label: 'Customers', icon: '👥', route: '/sales/customers' },
        { label: 'Add Customer', icon: '➕', route: '/sales/customers/create' },
        { label: 'Sales Orders', icon: '🛒', route: '/sales/orders' },
        { label: 'New Order', icon: '➕', route: '/sales/orders/create' },
        { label: 'Invoices', icon: '📄', route: '/sales/invoices' },
      ],
    },
    {
      label: 'Purchases',
      icon: '🛒',
      route: '/purchases',
      expanded: false,
      children: [
        { label: 'Vendors', icon: '🏢', route: '/purchases/vendors' },
        { label: 'Add Vendor', icon: '➕', route: '/purchases/vendors/new' },
        { label: 'Purchase Orders', icon: '📋', route: '/purchases/purchase-orders' },
        { label: 'New PO', icon: '➕', route: '/purchases/purchase-orders/new' },
        { label: 'Goods Receipt', icon: '📦', route: '/purchases/goods-receipt-notes' },
      ],
    },
    {
      label: 'Reports',
      icon: '📊',
      route: '/reports',
      expanded: false,
      children: [
        { label: 'Sales Report', icon: '💰', route: '/reports/sales' },
        { label: 'Purchase Report', icon: '🛍️', route: '/reports/purchases' },
        { label: 'Inventory Report', icon: '📦', route: '/reports/inventory' },
      ],
    },
    {
      label: 'Users',
      icon: '👥',
      route: '/users',
      expanded: false,
      children: [
        { label: 'All Users', icon: '👤', route: '/users' },
        { label: 'Add User', icon: '➕', route: '/users/create' },
        { label: 'My Profile', icon: '⚙️', route: '/users/profile' },
        { label: 'Activity Logs', icon: '📜', route: '/users/activity-logs' },
      ],
    },
  ];

  constructor(private router: Router) {
    this.loadCurrentUser();
  }

  loadCurrentUser(): void {
    const userJson = localStorage.getItem('currentUser');
    if (userJson) {
      this.currentUser = JSON.parse(userJson);
    }
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  toggleMenuItem(item: any): void {
    if (item.children) {
      item.expanded = !item.expanded;
    } else {
      this.router.navigate([item.route]);
    }
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.router.navigate(['/login']);
  }

  getPageTitle(): string {
    const url = this.router.url;
    if (url.includes('/dashboard')) return 'Dashboard';
    if (url.includes('/products/create')) return 'Add Product';
    if (url.includes('/products/edit')) return 'Edit Product';
    if (url.includes('/products/categories')) return 'Categories';
    if (url.includes('/products/brands')) return 'Brands';
    if (url.includes('/products')) return 'Products';
    if (url.includes('/sales/customers/create')) return 'Add Customer';
    if (url.includes('/sales/customers/edit')) return 'Edit Customer';
    if (url.includes('/sales/customers')) return 'Customers';
    if (url.includes('/sales/orders/create')) return 'Create Sales Order';
    if (url.includes('/sales/orders/edit')) return 'Edit Sales Order';
    if (url.includes('/sales/orders')) return 'Sales Orders';
    if (url.includes('/sales/invoices')) return 'Invoices';
    if (url.includes('/purchases/vendors/new')) return 'Add Vendor';
    if (url.includes('/purchases/vendors') && url.includes('/edit')) return 'Edit Vendor';
    if (url.includes('/purchases/vendors')) return 'Vendors';
    if (url.includes('/purchases/purchase-orders/new')) return 'New Purchase Order';
    if (url.includes('/purchases/purchase-orders') && url.includes('/edit')) return 'Edit Purchase Order';
    if (url.includes('/purchases/purchase-orders')) return 'Purchase Orders';
    if (url.includes('/purchases/goods-receipt-notes/new')) return 'New GRN';
    if (url.includes('/purchases/goods-receipt-notes')) return 'Goods Receipt Notes';
    if (url.includes('/reports/sales')) return 'Sales Report';
    if (url.includes('/reports/purchases')) return 'Purchase Report';
    if (url.includes('/reports/inventory')) return 'Inventory Report';
    if (url.includes('/users/create')) return 'Add User';
    if (url.includes('/users/edit')) return 'Edit User';
    if (url.includes('/users/profile')) return 'My Profile';
    if (url.includes('/users/activity-logs')) return 'Activity Logs';
    if (url.includes('/users') && url.includes('/activity')) return 'User Activity';
    if (url.includes('/users')) return 'Users';
    return 'InventoryProc';
  }
}
