import { HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { BoardCard, PagedResult } from './board.models';

// Reads the authenticated hire's own board. The auth interceptor attaches the
// token and the backend resolves the hire from the JWT sub claim — no hire id
// is ever sent from the client.
@Injectable({ providedIn: 'root' })
export class BoardService {
  private readonly api = inject(ApiService);

  // The Kanban renders the whole board at once, so ask for the maximum page
  // the backend validator allows (100) sorted by card order — an onboarding
  // board is a handful of cards, nowhere near that ceiling.
  getMyBoard(): Observable<PagedResult<BoardCard>> {
    const params = new HttpParams().set('sort', 'asc').set('pageSize', 100);
    return this.api.get<PagedResult<BoardCard>>('boards/me', params);
  }
}
