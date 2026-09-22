import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './shared/layouts/main-layout/main-layout.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'products',
        loadChildren: () => import('./features/products/products.routes').then(m => m.PRODUCTS_ROUTES)
      },
      {
        path: 'sales',
        loadChildren: () => import('./features/sales/sales.routes').then(m => m.SALES_ROUTES)
      },
      {
        path: 'purchases',
        loadChildren: () => import('./features/purchases/purchases.routes').then(m => m.PURCHASES_ROUTES)
      },
      {
        path: 'reports',
        loadChildren: () => import('./features/reports/reports.routes').then(m => m.REPORTS_ROUTES)
      },
      {
        path: 'users',
        loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES)
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
