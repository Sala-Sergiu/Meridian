import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BoardCard } from '../board/board.models';
import { ResourcePage } from './resource-page';

function card(partial: Partial<BoardCard> & { id: number; url: string }): BoardCard {
  return {
    title: 'Card',
    description: 'Description',
    type: 'Resource',
    order: 1,
    status: 'ToDo',
    ...partial,
  };
}

describe('ResourcePage', () => {
  let fixture: ComponentFixture<ResourcePage>;
  let controller: HttpTestingController;

  async function render(slug: string, boardCards: BoardCard[] | 'no-board'): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [ResourcePage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug })) } },
      ],
    }).compileComponents();

    controller = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(ResourcePage);
    fixture.detectChanges(); // ngOnInit -> board fetch

    const boardReq = controller.expectOne((r) => r.url === `${environment.apiBaseUrl}/boards/me`);
    if (boardCards === 'no-board') {
      boardReq.flush(null, { status: 404, statusText: 'Not Found' });
    } else {
      boardReq.flush({
        items: boardCards,
        page: 1,
        pageSize: 100,
        totalCount: boardCards.length,
        totalPages: 1,
      });
    }
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  afterEach(() => controller.verify());

  it('renders the content and the not-found state', async () => {
    const element = await render('safety-basics', 'no-board');

    expect(element.querySelector('h2')?.textContent).toBe('Workplace safety basics');
    expect(element.textContent).toContain('Evacuation routes');
    // No board -> article only, no acknowledgment flow.
    expect(element.querySelector('.acknowledge')).toBeNull();
  });

  it('opening a ToDo task automatically moves it to InProgress', async () => {
    const element = await render('dev-setup', [
      card({ id: 3, url: '/resources/dev-setup', type: 'Resource', status: 'ToDo' }),
    ]);

    const req = controller.expectOne(`${environment.apiBaseUrl}/boards/me/cards/3`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'InProgress' });
    req.flush(card({ id: 3, url: '/resources/dev-setup', status: 'InProgress' }));
    fixture.detectChanges();

    // Still not done, so the mark-as-done button is offered at the end.
    expect(element.querySelector('.acknowledge button')?.textContent).toContain('Mark as done');
  });

  it('does not auto-start safety cards, and marks them read from the article', async () => {
    const element = await render('safety-basics', [
      card({ id: 5, url: '/resources/safety-basics', type: 'Safety', status: 'ToDo' }),
    ]);

    controller.expectNone((r) => r.method === 'PATCH');
    const button = element.querySelector<HTMLButtonElement>('.acknowledge button')!;
    expect(button.textContent).toContain('Mark as read');

    button.click();
    fixture.detectChanges();

    controller
      .expectOne(`${environment.apiBaseUrl}/boards/me/cards/5`)
      .flush(card({ id: 5, url: '/resources/safety-basics', type: 'Safety', status: 'Done' }));
    fixture.detectChanges();

    expect(element.querySelector('.done-state')?.textContent).toContain('✓ Read');
    expect(element.querySelector('.acknowledge button')).toBeNull();
  });

  it('shows the done state for an already-completed card', async () => {
    const element = await render('dev-setup', [
      card({ id: 3, url: '/resources/dev-setup', type: 'Resource', status: 'Done' }),
    ]);

    controller.expectNone((r) => r.method === 'PATCH');
    expect(element.querySelector('.done-state')?.textContent).toContain('✓ Done');
  });

  it('contact pages get no acknowledgment flow', async () => {
    const element = await render('it-helpdesk', [
      card({ id: 7, url: '/resources/it-helpdesk', type: 'Contact', status: 'ToDo' }),
    ]);

    controller.expectNone((r) => r.method === 'PATCH');
    expect(element.querySelector('h2')?.textContent).toBe('IT helpdesk');
    expect(element.querySelector('.acknowledge')).toBeNull();
  });

  it('shows a friendly not-found state for an unknown slug', async () => {
    const element = await render('no-such-resource', 'no-board');

    expect(element.querySelector('h2')?.textContent).toBe('Resource not found');
    expect(element.querySelector('a.back')).not.toBeNull();
  });
});
