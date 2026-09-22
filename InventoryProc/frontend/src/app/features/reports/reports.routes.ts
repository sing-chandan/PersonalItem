import { Routes } from '@angular/router';

export const REPORTS_ROUTES: Routes = [
  {
    path: 'sales',
    loadComponent: () => import('./components/sales-report/sales-report.component')
      .then(m => m.SalesReportComponent)
  },
  {
    path: 'purchases',
    loadComponent: () => import('./components/purchase-report/purchase-report.component')
      .then(m => m.PurchaseReportComponent)
  },
  {
    path: 'inventory',
    loadComponent: () => import('./components/inventory-report/inventory-report.component')
      .then(m => m.InventoryReportComponent)
  },
  {
    path: '',
    redirectTo: 'sales',
    pathMatch: 'full'
  }
];
