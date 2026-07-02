import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthState } from './auth-state';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let auth: AuthState;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthState);
  });

  afterEach(() => controller.verify());

  it('attaches a bearer token to API requests when a token is present', () => {
    auth.setToken('jwt-123');

    http.get(`${environment.apiBaseUrl}/users/me`).subscribe();

    const req = controller.expectOne(`${environment.apiBaseUrl}/users/me`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer jwt-123');
    req.flush({});
  });

  it('sends no Authorization header without a token', () => {
    http.get(`${environment.apiBaseUrl}/users/me`).subscribe();

    const req = controller.expectOne(`${environment.apiBaseUrl}/users/me`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('never attaches the token to non-API requests', () => {
    auth.setToken('jwt-123');

    http.get('https://example.com/other').subscribe();

    const req = controller.expectOne('https://example.com/other');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });
});
