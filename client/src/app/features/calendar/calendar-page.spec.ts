import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { CalendarDay, CalendarMonth } from './calendar.models';
import { CalendarPage } from './calendar-page';

// June 2026: starts on a Monday, 30 days.
function juneDays(): CalendarDay[] {
  return Array.from({ length: 30 }, (_, i) => {
    const dayOfMonth = i + 1;
    const weekday = new Date(2026, 5, dayOfMonth).getDay(); // 0 = Sunday
    const isWeekend = weekday === 0 || weekday === 6;
    return {
      date: `2026-06-${String(dayOfMonth).padStart(2, '0')}`,
      kind: dayOfMonth === 1 ? 'Holiday' : isWeekend ? 'Weekend' : weekday <= 3 ? 'Office' : 'Remote',
      holidayName: dayOfMonth === 1 ? 'Pentecost Monday & Children’s Day' : null,
    };
  });
}

function flushMonth(controller: HttpTestingController, month: CalendarMonth): void {
  controller.expectOne((r) => r.url === `${environment.apiBaseUrl}/calendar/me`).flush(month);
}

describe('CalendarPage', () => {
  let fixture: ComponentFixture<CalendarPage>;
  let controller: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    controller = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(CalendarPage);
    fixture.detectChanges(); // ngOnInit -> load current month
  });

  afterEach(() => controller.verify());

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  it('requests the current month with year and month params', () => {
    const req = controller.expectOne((r) => r.url === `${environment.apiBaseUrl}/calendar/me`);
    const now = new Date();
    expect(req.request.params.get('year')).toBe(String(now.getFullYear()));
    expect(req.request.params.get('month')).toBe(String(now.getMonth() + 1));
    req.flush({ year: now.getFullYear(), month: now.getMonth() + 1, days: [] });
  });

  it('renders the days with their kinds and the holiday name', () => {
    flushMonth(controller, { year: 2026, month: 6, days: juneDays() });
    fixture.detectChanges();

    const cells = element().querySelectorAll('.cell:not(.blank)');
    expect(cells.length).toBe(30);
    expect(element().querySelectorAll('.cell.kind-office').length).toBeGreaterThan(0);
    expect(element().querySelectorAll('.cell.kind-remote').length).toBeGreaterThan(0);
    expect(element().textContent).toContain('Pentecost Monday');
  });

  it('navigating months fetches the shifted month', () => {
    flushMonth(controller, { year: 2026, month: 6, days: [] });
    fixture.detectChanges();

    element().querySelector<HTMLButtonElement>('.month-nav button')!.click(); // previous
    const req = controller.expectOne((r) => r.url === `${environment.apiBaseUrl}/calendar/me`);
    const requestedMonth = Number(req.request.params.get('month'));
    expect(requestedMonth).toBeGreaterThanOrEqual(1);
    expect(requestedMonth).toBeLessThanOrEqual(12);
    req.flush({ year: 2026, month: requestedMonth, days: [] });
  });

  it('shows the correlation id when the calendar fails to load', () => {
    controller.expectOne((r) => r.url === `${environment.apiBaseUrl}/calendar/me`).flush(
      { title: 'error', status: 500, correlationId: 'corr-cal-1' },
      { status: 500, statusText: 'Internal Server Error' },
    );
    fixture.detectChanges();

    expect(element().textContent).toContain('Could not load your calendar.');
    expect(element().textContent).toContain('corr-cal-1');
  });
});
