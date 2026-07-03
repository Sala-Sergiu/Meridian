import { Injectable, computed, signal } from '@angular/core';
import { AuthUser, LoginResult } from './auth.models';

const STORAGE_KEY = 'meridian.session';

// Reads the persisted session once at startup so a page refresh keeps the
// user logged in. Defensive: anything unreadable in storage counts as
// logged out rather than crashing the app.
function readStoredSession(): LoginResult | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw === null) {
      return null;
    }

    const session = JSON.parse(raw) as LoginResult;
    return typeof session.token === 'string' && typeof session.user?.id === 'number'
      ? session
      : null;
  } catch {
    return null;
  }
}

// Signal-based session store (no NgRx — decided). Holds the JWT the auth
// interceptor attaches to API requests, plus the logged-in user for the UI.
//
// TRADE-OFF: the session lives in localStorage for simplicity — it survives
// refreshes but is readable by any script on the page, so an XSS hole exposes
// the token. httpOnly cookies would be more XSS-resistant; documented as a
// known trade-off, not implemented now.
@Injectable({ providedIn: 'root' })
export class AuthState {
  private readonly sessionSignal = signal<LoginResult | null>(readStoredSession());

  readonly token = computed(() => this.sessionSignal()?.token ?? null);
  readonly user = computed<AuthUser | null>(() => this.sessionSignal()?.user ?? null);
  readonly isAuthenticated = computed(() => this.sessionSignal() !== null);

  setSession(session: LoginResult): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    this.sessionSignal.set(session);
  }

  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.sessionSignal.set(null);
  }
}
