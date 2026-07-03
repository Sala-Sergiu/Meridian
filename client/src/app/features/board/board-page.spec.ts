import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthState } from '../../core/auth/auth-state';
import { environment } from '../../../environments/environment';
import { BoardCard } from './board.models';
import { BoardPage } from './board-page';

function card(partial: Partial<BoardCard> & { id: number }): BoardCard {
  return {
    title: `Card ${partial.id}`,
    description: 'Description',
    type: 'Resource',
    url: null,
    order: partial.id,
    status: 'ToDo',
    ...partial,
  };
}

function flushBoard(controller: HttpTestingController, items: BoardCard[]): void {
  controller
    .expectOne((r) => r.url === `${environment.apiBaseUrl}/boards/me`)
    .flush({ items, page: 1, pageSize: 100, totalCount: items.length, totalPages: 1 });
}

describe('BoardPage', () => {
  let fixture: ComponentFixture<BoardPage>;
  let controller: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [BoardPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    controller = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(BoardPage);
    fixture.detectChanges(); // triggers ngOnInit -> load
  });

  afterEach(() => {
    controller.verify();
    localStorage.clear();
  });

  function columnCards(): Record<string, string[]> {
    const element = fixture.nativeElement as HTMLElement;
    const result: Record<string, string[]> = {};
    element.querySelectorAll('.column').forEach((column) => {
      const label = column.querySelector('h3')!.textContent!.trim();
      result[label.replace(/\s+\d+$/, '').trim()] = Array.from(
        column.querySelectorAll('.card h4'),
        (h) => h.textContent!.trim(),
      );
    });
    return result;
  }

  it('buckets cards into the three status columns sorted by order', () => {
    flushBoard(controller, [
      card({ id: 3, status: 'ToDo', order: 3 }),
      card({ id: 1, status: 'ToDo', order: 1 }),
      card({ id: 2, status: 'InProgress', order: 2 }),
      card({ id: 4, status: 'Done', order: 4 }),
    ]);
    fixture.detectChanges();

    const columns = columnCards();
    expect(columns['To do']).toEqual(['Card 1', 'Card 3']);
    expect(columns['In progress']).toEqual(['Card 2']);
    expect(columns['Done']).toEqual(['Card 4']);
  });

  it('treats a 404 (no board assigned) as an empty board, not an error', () => {
    controller
      .expectOne((r) => r.url === `${environment.apiBaseUrl}/boards/me`)
      .flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('No onboarding board has been assigned to you yet');
    expect(element.querySelectorAll('.column').length).toBe(3);
    expect(element.querySelectorAll('.card').length).toBe(0);
    expect(element.querySelector('.error')).toBeNull();
  });

  it('shows a loading state until the board arrives', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Loading your board');

    flushBoard(controller, []);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Loading your board');
  });

  it('renders a color-coded type badge and a safe external link per card', () => {
    flushBoard(controller, [
      card({ id: 1, type: 'Safety', url: 'https://intranet.local/evacuation' }),
    ]);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const badge = element.querySelector('.type-safety');
    expect(badge?.textContent?.trim()).toBe('Safety');

    const link = element.querySelector<HTMLAnchorElement>('.card a')!;
    expect(link.href).toBe('https://intranet.local/evacuation');
    expect(link.target).toBe('_blank');
    expect(link.rel).toBe('noopener');
  });

  it('shows a per-column empty state', () => {
    flushBoard(controller, [card({ id: 1, status: 'ToDo' })]);
    fixture.detectChanges();

    const empties = (fixture.nativeElement as HTMLElement).querySelectorAll('.empty');
    expect(empties.length).toBe(2);
    expect(empties[0].textContent).toContain('Nothing here yet');
  });

  it('shows the correlation id from ProblemDetails when the request fails', () => {
    controller.expectOne((r) => r.url === `${environment.apiBaseUrl}/boards/me`).flush(
      {
        type: 'https://httpstatuses.io/500',
        title: 'An unexpected error occurred.',
        status: 500,
        correlationId: 'corr-123',
      },
      { status: 500, statusText: 'Internal Server Error' },
    );
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('Could not load your board.');
    expect(text).toContain('corr-123');
  });

  // jsdom cannot perform a real pointer drag, so drop tests synthesize the
  // CdkDragDrop event the directive would emit and call the handler directly.
  function drop(dragged: BoardCard, from: string, to: string): void {
    const containers: Record<string, { data: string }> = {
      [from]: { data: from },
      [to]: { data: to },
    };
    const event = {
      previousContainer: containers[from],
      container: containers[to],
      item: { data: dragged },
    };
    (fixture.componentInstance as unknown as { onDrop(e: unknown): void }).onDrop(event);
    fixture.detectChanges();
  }

  it('dropping a card into another column patches the move', () => {
    const dragged = card({ id: 1, status: 'ToDo' });
    flushBoard(controller, [dragged]);
    fixture.detectChanges();

    drop(dragged, 'ToDo', 'InProgress');

    const req = controller.expectOne(`${environment.apiBaseUrl}/boards/me/cards/1`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'InProgress' });
    req.flush({ ...dragged, status: 'InProgress' });
    fixture.detectChanges();

    const columns = columnCards();
    expect(columns['To do']).toEqual([]);
    expect(columns['In progress']).toEqual(['Card 1']);
  });

  it('dropping a card into its own column is a no-op', () => {
    const dragged = card({ id: 1, status: 'ToDo' });
    flushBoard(controller, [dragged]);
    fixture.detectChanges();

    const container = { data: 'ToDo' };
    (fixture.componentInstance as unknown as { onDrop(e: unknown): void }).onDrop({
      previousContainer: container,
      container,
      item: { data: dragged },
    });

    controller.expectNone(`${environment.apiBaseUrl}/boards/me/cards/1`);
  });

  it('shows the logged-in user and logs out to /login', () => {
    TestBed.inject(AuthState).setSession({
      token: 'jwt-abc',
      user: { id: 1, email: 'newhire@meridian.local', displayName: 'Nadia NewHire', role: 'NewHire' },
    });
    flushBoard(controller, []);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.whoami')?.textContent).toContain('Nadia NewHire');

    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    element.querySelector<HTMLButtonElement>('.whoami button')!.click();

    expect(TestBed.inject(AuthState).isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith('/login');
  });
});
