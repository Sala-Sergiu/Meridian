import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthState } from './auth-state';

// Attaches "Authorization: Bearer <token>" to outgoing API requests when a
// token is present. Scoped to apiBaseUrl so the token never leaks to third
// parties.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthState).token();

  if (token === null || !req.url.startsWith(environment.apiBaseUrl)) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
