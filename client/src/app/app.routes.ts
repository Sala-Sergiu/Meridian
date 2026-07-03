import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

// /login is public; everything product-facing is guarded. The board itself is
// still a placeholder — the real screen lands in a later slice. Lazy-loaded
// standalone components.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'board' },
  {
    path: 'login',
    title: 'Sign in · Meridian',
    loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'board',
    title: 'My onboarding · Meridian',
    canActivate: [authGuard],
    loadComponent: () => import('./features/board/board-page').then((m) => m.BoardPage),
  },
  { path: '**', redirectTo: 'board' },
];
