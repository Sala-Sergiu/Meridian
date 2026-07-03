import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthState } from './auth-state';
import { LoginResult } from './auth.models';
import { AuthService } from './auth.service';

const LOGIN_RESULT: LoginResult = {
  token: 'jwt-abc',
  user: { id: 1, email: 'newhire@meridian.local', displayName: 'Nadia NewHire', role: 'NewHire' },
};

describe('AuthService', () => {
  let service: AuthService;
  let state: AuthState;
  let controller: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    state = TestBed.inject(AuthState);
    controller = TestBed.inject(HttpTestingController);
    TestBed.inject(HttpClient);
  });

  afterEach(() => {
    controller.verify();
    localStorage.clear();
  });

  it('posts credentials to the login endpoint and stores the session', () => {
    service.login({ email: 'newhire@meridian.local', password: 'pw' }).subscribe();

    const req = controller.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'newhire@meridian.local', password: 'pw' });
    req.flush(LOGIN_RESULT);

    expect(state.isAuthenticated()).toBe(true);
    expect(state.token()).toBe('jwt-abc');
    expect(state.user()?.role).toBe('NewHire');
  });

  it('stores nothing when the login fails', () => {
    let failed = false;
    service.login({ email: 'newhire@meridian.local', password: 'wrong' }).subscribe({
      error: () => (failed = true),
    });

    controller
      .expectOne(`${environment.apiBaseUrl}/auth/login`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(failed).toBe(true);
    expect(state.isAuthenticated()).toBe(false);
  });

  it('logout clears the session', () => {
    state.setSession(LOGIN_RESULT);

    service.logout();

    expect(state.isAuthenticated()).toBe(false);
    expect(state.token()).toBeNull();
  });
});
