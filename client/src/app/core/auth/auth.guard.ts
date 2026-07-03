import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthState } from './auth-state';

// Guards authenticated routes: allow when logged in, otherwise redirect to
// /login by returning a UrlTree (not just false, which would strand the user
// on a blank page).
//
// SECURITY NOTE: this guard is UX, not a security boundary — it only decides
// what the browser renders. The backend authorization policies are the real
// enforcement on every API call.
export const authGuard: CanActivateFn = () => {
  const authState = inject(AuthState);
  const router = inject(Router);

  return authState.isAuthenticated() ? true : router.parseUrl('/login');
};
