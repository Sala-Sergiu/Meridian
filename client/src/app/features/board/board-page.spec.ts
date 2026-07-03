import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
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

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function columnCards(): Record<string, string[]> {
    const result: Record<string, string[]> = {};
    element().querySelectorAll('.column').forEach((column) => {
      const label = column.querySelector('h4')!.textContent!.trim();
      result[label.replace(/\s+\d+$/, '').trim()] = Array.from(
        column.querySelectorAll('.card h5'),
        (h) => h.textContent!.trim(),
      );
    });
    return result;
  }

  it('buckets task cards into the three status columns sorted by order', () => {
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

  it('shows safety cards under requires attention, not in the kanban', () => {
    flushBoard(controller, [
      card({ id: 1, type: 'Safety', title: 'Safety rules' }),
      card({ id: 2, type: 'Resource', title: 'Handbook' }),
    ]);
    fixture.detectChanges();

    const attention = element().querySelector('.attention')!;
    expect(attention.textContent).toContain('Safety rules');
    expect(attention.textContent).toContain('0 of 1 read');
    expect(columnCards()['To do']).toEqual(['Handbook']);
  });

  it('shows read/unread state on attention items without any board-side button', () => {
    flushBoard(controller, [
      card({ id: 5, type: 'Safety', title: 'Safety rules', status: 'ToDo', url: '/resources/safety-basics' }),
      card({ id: 6, type: 'Safety', title: 'Confidentiality', status: 'Done', url: '/resources/data-confidentiality' }),
    ]);
    fixture.detectChanges();

    const attention = element().querySelector('.attention')!;
    // Cards with an in-app article are acknowledged there, not from the board.
    expect(attention.querySelector('button')).toBeNull();
    expect(attention.querySelector('.unread-state')).not.toBeNull();
    expect(attention.querySelector('.read-state')?.textContent).toContain('✓ Read');
    expect(attention.textContent).toContain('1 of 2 read');
  });

  it('lets required reading without an in-app article be marked read on the board', () => {
    flushBoard(controller, [
      card({ id: 8, type: 'Safety', title: 'GDPR refresher', status: 'ToDo', url: null }),
    ]);
    fixture.detectChanges();

    const button = element().querySelector<HTMLButtonElement>('.attention .mark-read')!;
    expect(button.textContent).toContain('Mark as read');

    button.click();
    fixture.detectChanges();

    const req = controller.expectOne(`${environment.apiBaseUrl}/boards/me/cards/8`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'Done' });
    req.flush(card({ id: 8, type: 'Safety', status: 'Done' }));
    fixture.detectChanges();

    const attention = element().querySelector('.attention')!;
    expect(attention.querySelector('.mark-read')).toBeNull();
    expect(attention.querySelector('.read-state')?.textContent).toContain('✓ Read');
  });

  it('renders contacts as reference info without any status control', () => {
    flushBoard(controller, [
      card({ id: 1, type: 'Contact', title: 'IT helpdesk', url: '/resources/it-helpdesk' }),
    ]);
    fixture.detectChanges();

    const contacts = element().querySelector('.contacts')!;
    expect(contacts.textContent).toContain('IT helpdesk');
    expect(contacts.querySelector('a')?.getAttribute('href')).toBe('/resources/it-helpdesk');
    expect(contacts.querySelector('button')).toBeNull();
    expect(element().querySelectorAll('.card').length).toBe(0);
  });

  it('shows task progress and celebrates when everything is done and read', () => {
    flushBoard(controller, [
      card({ id: 1, type: 'Resource', status: 'Done' }),
      card({ id: 2, type: 'Resource', status: 'Done' }),
      card({ id: 3, type: 'Safety', status: 'Done' }),
      card({ id: 4, type: 'Contact', status: 'ToDo' }), // contacts never count
    ]);
    fixture.detectChanges();

    expect(element().querySelector('.progress-label')?.textContent).toContain('2 of 2 done');
    expect(element().querySelector('.all-done')).not.toBeNull();
  });

  it('does not celebrate while required reading is unread', () => {
    flushBoard(controller, [
      card({ id: 1, type: 'Resource', status: 'Done' }),
      card({ id: 2, type: 'Safety', status: 'ToDo' }),
    ]);
    fixture.detectChanges();

    expect(element().querySelector('.all-done')).toBeNull();
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

  it('dropping a task into another column moves it optimistically, then patches', () => {
    const dragged = card({ id: 1, status: 'ToDo' });
    flushBoard(controller, [dragged]);
    fixture.detectChanges();

    drop(dragged, 'ToDo', 'InProgress');

    // The card has already jumped columns BEFORE the server answers.
    expect(columnCards()['In progress']).toEqual(['Card 1']);

    const req = controller.expectOne(`${environment.apiBaseUrl}/boards/me/cards/1`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'InProgress' });
    req.flush({ ...dragged, status: 'InProgress' });
    fixture.detectChanges();

    const columns = columnCards();
    expect(columns['To do']).toEqual([]);
    expect(columns['In progress']).toEqual(['Card 1']);
  });

  it('reverts the card and shows the correlation id when the move fails', () => {
    const dragged = card({ id: 1, status: 'ToDo' });
    flushBoard(controller, [dragged]);
    fixture.detectChanges();

    drop(dragged, 'ToDo', 'Done');
    expect(columnCards()['Done']).toEqual(['Card 1']);

    controller.expectOne(`${environment.apiBaseUrl}/boards/me/cards/1`).flush(
      {
        type: 'https://httpstatuses.io/500',
        title: 'An unexpected error occurred.',
        status: 500,
        correlationId: 'corr-move-9',
      },
      { status: 500, statusText: 'Internal Server Error' },
    );
    fixture.detectChanges();

    const columns = columnCards();
    expect(columns['To do']).toEqual(['Card 1']);
    expect(columns['Done']).toEqual([]);

    const banner = element().querySelector('.move-error')!;
    expect(banner.textContent).toContain('it has been put back');
    expect(banner.textContent).toContain('corr-move-9');

    banner.querySelector<HTMLButtonElement>('button')!.click();
    fixture.detectChanges();
    expect(element().querySelector('.move-error')).toBeNull();
  });

  it('marks the moved card as saving while the PATCH is in flight', () => {
    const dragged = card({ id: 1, status: 'ToDo' });
    flushBoard(controller, [dragged]);
    fixture.detectChanges();

    drop(dragged, 'ToDo', 'InProgress');

    expect(element().querySelector('.card.saving')).not.toBeNull();

    controller
      .expectOne(`${environment.apiBaseUrl}/boards/me/cards/1`)
      .flush({ ...dragged, status: 'InProgress' });
    fixture.detectChanges();

    expect(element().querySelector('.card.saving')).toBeNull();
  });

  it('dropping a task into its own column is a no-op', () => {
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

  it('renders an in-app router link for relative card urls', () => {
    flushBoard(controller, [card({ id: 1, url: '/resources/dev-setup' })]);
    fixture.detectChanges();

    const link = element().querySelector<HTMLAnchorElement>('.card a')!;
    expect(link.getAttribute('href')).toBe('/resources/dev-setup');
    expect(link.textContent).toContain('View details');
    expect(link.target).not.toBe('_blank');
  });

  it('shows a loading state until the board arrives', () => {
    expect(element().textContent).toContain('Loading your board');

    flushBoard(controller, []);
    fixture.detectChanges();

    expect(element().textContent).not.toContain('Loading your board');
  });

  it('treats a 404 (no board assigned) as an empty board, not an error', () => {
    controller
      .expectOne((r) => r.url === `${environment.apiBaseUrl}/boards/me`)
      .flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();

    expect(element().textContent).toContain('No onboarding board has been assigned to you yet');
    expect(element().querySelectorAll('.column').length).toBe(3);
    expect(element().querySelectorAll('.card').length).toBe(0);
    expect(element().querySelector('app-error-banner')).toBeNull();
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

    const text = element().textContent!;
    expect(text).toContain('Could not load your board.');
    expect(text).toContain('corr-123');
  });
});
