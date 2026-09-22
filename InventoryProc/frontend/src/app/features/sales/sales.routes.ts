import { Routes } from '@angular/router';

export const SALES_ROUTES: Routes = [
  {
    path: 'customers',
    loadComponent: () =>
      import('./pages/customer-list/customer-list.component').then(
        (m) => m.CustomerListComponent
      ),
  },
  {
    path: 'customers/create',
    loadComponent: () =>
      import('./pages/customer-form/customer-form.component').then(
        (m) => m.CustomerFormComponent
      ),
  },
  {
    path: 'customers/edit/:id',
    loadComponent: () =>
      import('./pages/customer-form/customer-form.component').then(
        (m) => m.CustomerFormComponent
      ),
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('./pages/sales-order-list/sales-order-list.component').then(
        (m) => m.SalesOrderListComponent
      ),
  },
  {
    path: 'orders/create',
    loadComponent: () =>
      import('./pages/sales-order-form/sales-order-form.component').then(
        (m) => m.SalesOrderFormComponent
      ),
  },
  {
    path: 'orders/edit/:id',
    loadComponent: () =>
      import('./pages/sales-order-form/sales-order-form.component').then(
        (m) => m.SalesOrderFormComponent
      ),
  },
  {
    path: 'orders/:id',
    loadComponent: () =>
      import('./pages/sales-order-detail/sales-order-detail.component').then(
        (m) => m.SalesOrderDetailComponent
      ),
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./pages/invoice-list/invoice-list.component').then(
        (m) => m.InvoiceListComponent
      ),
  },
  {
    path: 'invoices/:id',
    loadComponent: () =>
      import('./pages/invoice-detail/invoice-detail.component').then(
        (m) => m.InvoiceDetailComponent
      ),
  },
  {
    path: '',
    redirectTo: 'customers',
    pathMatch: 'full',
  },
];
