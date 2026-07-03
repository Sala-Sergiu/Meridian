import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { BoardCard, PagedResult } from './board.models';
import { BoardService } from './board.service';

describe('BoardService', () => {
  let service: BoardService;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(BoardService);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('fetches the own board sorted by order without any hire id', () => {
    let result: PagedResult<BoardCard> | undefined;
    service.getMyBoard().subscribe((page) => (result = page));

    const req = controller.expectOne(
      (r) => r.url === `${environment.apiBaseUrl}/boards/me` && r.method === 'GET',
    );
    expect(req.request.params.get('sort')).toBe('asc');
    expect(req.request.params.get('pageSize')).toBe('100');
    expect(req.request.params.has('hireUserId')).toBe(false);

    const page: PagedResult<BoardCard> = {
      items: [
        {
          id: 1,
          title: 'Read the handbook',
          description: 'Company handbook',
          type: 'Resource',
          url: 'https://intranet.local/handbook',
          order: 1,
          status: 'ToDo',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      totalPages: 1,
    };
    req.flush(page);

    expect(result?.items.length).toBe(1);
    expect(result?.items[0].type).toBe('Resource');
  });
});
