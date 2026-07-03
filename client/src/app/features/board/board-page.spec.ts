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
    expect(element.querySelectorAll('.column').length).toBe(3);
    expect(element.querySelectorAll('.card').length).toBe(0);
  });

});
