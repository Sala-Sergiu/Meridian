import { Routes } from '@angular/router';

// Routing shell: placeholder routes only, wired so later slices drop real
// screens in without touching the shell. Lazy-loaded standalone components.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'board' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'board',
    loadComponent: () => import('./features/board/board-page').then((m) => m.BoardPage),
  },
  { path: '**', redirectTo: 'board' },
];
