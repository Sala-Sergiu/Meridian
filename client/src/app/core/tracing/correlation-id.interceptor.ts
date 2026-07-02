import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const CORRELATION_ID_HEADER = 'X-Correlation-ID';

// Stamps every outgoing API request with a fresh GUID in X-Correlation-ID —
// the same header the backend's correlation middleware reads, propagates and
// enriches Serilog logs with. One id per request gives end-to-end tracing:
// a failed call in the browser can be matched to its exact backend log lines.
export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiBaseUrl)) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { [CORRELATION_ID_HEADER]: crypto.randomUUID() } }));
};
