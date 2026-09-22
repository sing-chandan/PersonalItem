import { Routes } from '@angular/router';

export const PURCHASES_ROUTES: Routes = [
  {
    path: 'vendors',
    loadComponent: () =>
      import('./components/vendor-list/vendor-list.component').then(
        (m) => m.VendorListComponent
      ),
  },
  {
    path: 'vendors/:id',
    loadComponent: () =>
      import('./components/vendor-form/vendor-form.component').then(
        (m) => m.VendorFormComponent
      ),
  },
  {
    path: 'vendors/:id/edit',
    loadComponent: () =>
      import('./components/vendor-form/vendor-form.component').then(
        (m) => m.VendorFormComponent
      ),
  },
  {
    path: 'purchase-orders',
    loadComponent: () =>
      import('./components/purchase-order-list/purchase-order-list.component').then(
        (m) => m.PurchaseOrderListComponent
      ),
  },
  {
    path: 'purchase-orders/:id',
    loadComponent: () =>
      import('./components/purchase-order-detail/purchase-order-detail.component').then(
        (m) => m.PurchaseOrderDetailComponent
      ),
  },
  {
    path: 'purchase-orders/:id/edit',
    loadComponent: () =>
      import('./components/purchase-order-form/purchase-order-form.component').then(
        (m) => m.PurchaseOrderFormComponent
      ),
  },
  {
    path: 'goods-receipt-notes',
    loadComponent: () =>
      import('./components/grn-list/grn-list.component').then(
        (m) => m.GRNListComponent
      ),
  },
  {
    path: 'goods-receipt-notes/:id',
    loadComponent: () =>
      import('./components/grn-list/grn-list.component').then(
        (m) => m.GRNListComponent
      ),
  },
  {
    path: 'goods-receipt-notes/new',
    loadComponent: () =>
      import('./components/grn-form/grn-form.component').then(
        (m) => m.GRNFormComponent
      ),
  },
];
