import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: 'list',
        loadComponent: () => import('./pages/product-list/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'create',
        loadComponent: () => import('./pages/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./pages/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./pages/categories/categories.component').then(m => m.CategoriesComponent)
      },
      {
        path: 'brands',
        loadComponent: () => import('./pages/brands/brands.component').then(m => m.BrandsComponent)
      },
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full'
      }
    ]
  }
];
