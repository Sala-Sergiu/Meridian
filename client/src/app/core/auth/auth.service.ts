import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from '../api/api.service';
import { AuthState } from './auth-state';
import { LoginRequest, LoginResult } from './auth.models';

// Login/logout against the backend. Goes through ApiService so the URL comes
// from environment config and the shared interceptors (correlation id; the
// bearer interceptor is a no-op here since login is anonymous) apply.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly authState = inject(AuthState);

  login(request: LoginRequest): Observable<LoginResult> {
    return this.api
      .post<LoginResult>('auth/login', request)
      .pipe(tap((result) => this.authState.setSession(result)));
  }

  logout(): void {
    this.authState.clear();
  }
}
