import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';

// loadComponent = lazy loading: o código de cada página só é baixado quando o usuário abre a página.
export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Entrar · FinanceControl',
    loadComponent: () => import('./features/auth/pages/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    title: 'Criar conta · FinanceControl',
    loadComponent: () => import('./features/auth/pages/register-page').then((m) => m.RegisterPage),
  },
  {
    // Rotas "dentro" do layout com barra de topo, todas protegidas pelo authGuard.
    path: '',
    loadComponent: () => import('./core/layout/shell').then((m) => m.Shell),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Dashboard · FinanceControl',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page').then((m) => m.DashboardPage),
      },
      {
        path: 'accounts',
        title: 'Contas · FinanceControl',
        loadComponent: () =>
          import('./features/accounts/pages/accounts-page').then((m) => m.AccountsPage),
      },
      {
        path: 'categories',
        title: 'Categorias · FinanceControl',
        loadComponent: () =>
          import('./features/categories/pages/categories-page').then((m) => m.CategoriesPage),
      },
    ],
  },
  {
    path: '**',
    title: 'Página não encontrada · FinanceControl',
    loadComponent: () =>
      import('./shared/pages/not-found-page').then((m) => m.NotFoundPage),
  },
];
