import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { BoardCard } from '../board/board.models';
import { HireBoard, HireProgress } from './hires.models';
import { HireBoardPage } from './hire-board-page';

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

function board(cards: BoardCard[]): HireBoard {
  return { id: 1, hireUserId: 7, assignedAt: '2026-07-01T00:00:00Z', cards };
}

const NADIA: HireProgress = {
  hireUserId: 7,
  displayName: 'Nadia',
  email: 'nadia@meridian.local',
  hasBoard: true,
  tasksDone: 1,
  tasksTotal: 2,
  readDone: 1,
  readTotal: 1,
};

describe('HireBoardPage', () => {
  let fixture: ComponentFixture<HireBoardPage>;
  let controller: HttpTestingController;

  async function render(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [HireBoardPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ hireUserId: '7' }) } },
        },
      ],
    }).compileComponents();

    controller = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(HireBoardPage);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  afterEach(() => {
    controller.verify();
  });

  it("shows the hire's reading and tasks grouped by column, read-only", async () => {
    const element = await render();

    controller.expectOne(`${environment.apiBaseUrl}/boards/7`).flush(
      board([
        card({ id: 1, title: 'Fire safety', type: 'Safety', status: 'Done' }),
        card({ id: 2, title: 'Set up laptop', status: 'Done' }),
        card({ id: 3, title: 'Meet the team', status: 'ToDo' }),
      ]),
    );
    controller.expectOne(`${environment.apiBaseUrl}/boards/progress`).flush([NADIA]);
    fixture.detectChanges();

    expect(element.querySelector('h2')!.textContent).toContain('Nadia — onboarding');
    expect(element.textContent).toContain('1 of 1 read');
    expect(element.textContent).toContain('1 of 2 done');

    const columns = element.querySelectorAll('.column');
    expect(columns.length).toBe(3);
    expect(columns[0].textContent).toContain('Meet the team');
    expect(columns[2].textContent).toContain('Set up laptop');

    // Read-only: no buttons, no drag handles anywhere on the board.
    expect(element.querySelector('button')).toBeNull();
    expect(element.querySelector('.cdk-drag')).toBeNull();
  });

  it('shows a friendly message when the hire has no board yet', async () => {
    const element = await render();

    controller.expectOne(`${environment.apiBaseUrl}/boards/progress`).flush([]);
    controller
      .expectOne(`${environment.apiBaseUrl}/boards/7`)
      .flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();

    expect(element.textContent).toContain('no onboarding board assigned yet');
    expect(element.querySelector('.kanban')).toBeNull();
  });

  it('surfaces the correlation id when loading fails', async () => {
    const element = await render();

    controller.expectOne(`${environment.apiBaseUrl}/boards/progress`).flush([]);
    controller
      .expectOne(`${environment.apiBaseUrl}/boards/7`)
      .flush({ correlationId: 'abc-123' }, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    expect(element.textContent).toContain('Could not load this board');
    expect(element.textContent).toContain('abc-123');
  });
});
