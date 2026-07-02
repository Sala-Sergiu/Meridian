import { Injectable, computed, signal } from '@angular/core';

// Signal-based token store (no NgRx — decided). Holds the JWT the auth
// interceptor attaches to API requests. Login/logout logic arrives with the
// login slice; this is just the store.
@Injectable({ providedIn: 'root' })
export class AuthState {
  private readonly tokenSignal = signal<string | null>(null);

  readonly token = this.tokenSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.tokenSignal() !== null);

  setToken(token: string): void {
    this.tokenSignal.set(token);
  }

  clearToken(): void {
    this.tokenSignal.set(null);
  }
}
