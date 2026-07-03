import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { AuthState } from './auth-state';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  afterEach(() => localStorage.clear());

  function runGuard(): boolean | UrlTree {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as boolean | UrlTree;
  }

  it('allows navigation when authenticated', () => {
    TestBed.inject(AuthState).setSession({
      token: 'jwt-abc',
      user: { id: 1, email: 'newhire@meridian.local', displayName: 'Nadia NewHire', role: 'NewHire' },
    });

    expect(runGuard()).toBe(true);
  });

  it('redirects to /login when logged out', () => {
    const result = runGuard();

    expect(result).toBeInstanceOf(UrlTree);
    expect(result.toString()).toBe('/login');
  });
});
