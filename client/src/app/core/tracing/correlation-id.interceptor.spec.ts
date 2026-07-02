import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { CORRELATION_ID_HEADER, correlationIdInterceptor } from './correlation-id.interceptor';

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

describe('correlationIdInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([correlationIdInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('stamps API requests with a GUID correlation id', () => {
    http.get(`${environment.apiBaseUrl}/templates`).subscribe();

    const req = controller.expectOne(`${environment.apiBaseUrl}/templates`);
    expect(req.request.headers.get(CORRELATION_ID_HEADER)).toMatch(GUID_PATTERN);
    req.flush([]);
  });

  it('uses a fresh id per request', () => {
    http.get(`${environment.apiBaseUrl}/templates`).subscribe();
    http.get(`${environment.apiBaseUrl}/templates`).subscribe();

    const requests = controller.match(`${environment.apiBaseUrl}/templates`);
    expect(requests.length).toBe(2);
    const [first, second] = requests.map((r) => r.request.headers.get(CORRELATION_ID_HEADER));
    expect(first).not.toBe(second);
    requests.forEach((r) => r.flush([]));
  });

  it('does not stamp non-API requests', () => {
    http.get('https://example.com/other').subscribe();

    const req = controller.expectOne('https://example.com/other');
    expect(req.request.headers.has(CORRELATION_ID_HEADER)).toBe(false);
    req.flush({});
  });
});
