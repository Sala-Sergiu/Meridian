import { HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { CalendarMonth } from './calendar.models';

// Reads the authenticated employee's own work calendar. The user is resolved
// server-side from the JWT — no user id is ever sent.
@Injectable({ providedIn: 'root' })
export class CalendarService {
  private readonly api = inject(ApiService);

  getMyMonth(year: number, month: number): Observable<CalendarMonth> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.api.get<CalendarMonth>('calendar/me', params);
  }
}
