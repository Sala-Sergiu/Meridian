import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { HireProgress, PublishCardRequest, PublishCardResult } from './hires.models';

// HR/Manager tracking and HR publishing. Authorization lives in the backend
// policies (HrOrManagerRead / HrWrite) — this service just calls.
@Injectable({ providedIn: 'root' })
export class HiresService {
  private readonly api = inject(ApiService);

  // The single seeded template; a template picker is future work.
  private readonly templateId = 1;

  getProgress(): Observable<HireProgress[]> {
    return this.api.get<HireProgress[]>('boards/progress');
  }

  assignBoard(hireUserId: number): Observable<unknown> {
    return this.api.post('boards/assign', { templateId: this.templateId, hireUserId });
  }

  publishCard(request: PublishCardRequest): Observable<PublishCardResult> {
    return this.api.post<PublishCardResult>(`templates/${this.templateId}/cards`, request);
  }
}
