import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorBanner } from '../../shared/ui/error-banner';
import { Loading } from '../../shared/ui/loading';
import { CalendarDay, CalendarMonth } from './calendar.models';
import { CalendarService } from './calendar.service';

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

interface CalendarCell {
  day: CalendarDay | null; // null = leading/trailing blank in the grid
  dayNumber: number | null;
  isToday: boolean;
}

// Month view of the employee's hybrid work calendar: office days, remote
// days, weekends and public holidays, straight from the backend contract —
// the frontend only lays the days out on a Monday-first grid.
@Component({
  selector: 'app-calendar-page',
  imports: [ErrorBanner, Loading],
  templateUrl: './calendar-page.html',
  styleUrl: './calendar-page.scss',
})
export class CalendarPage implements OnInit {
  private readonly calendarService = inject(CalendarService);

  protected readonly weekdays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  protected readonly year = signal(new Date().getFullYear());
  protected readonly month = signal(new Date().getMonth() + 1); // 1-based
  protected readonly loading = signal(true);
  protected readonly error = signal<{ message: string; correlationId: string | null } | null>(null);
  private readonly data = signal<CalendarMonth | null>(null);

  protected readonly monthLabel = computed(() => `${MONTH_NAMES[this.month() - 1]} ${this.year()}`);

  protected readonly cells = computed<CalendarCell[]>(() => {
    const monthData = this.data();
    if (monthData === null) {
      return [];
    }

    const today = toIsoDate(new Date());
    const firstWeekday = (new Date(monthData.year, monthData.month - 1, 1).getDay() + 6) % 7; // Mon = 0
    const blanks: CalendarCell[] = Array.from({ length: firstWeekday }, () => ({
      day: null,
      dayNumber: null,
      isToday: false,
    }));

    return blanks.concat(
      monthData.days.map((day, index) => ({
        day,
        dayNumber: index + 1,
        isToday: day.date === today,
      })),
    );
  });

  ngOnInit(): void {
    this.load();
  }

  protected previousMonth(): void {
    this.shiftMonth(-1);
  }

  protected nextMonth(): void {
    this.shiftMonth(1);
  }

  private shiftMonth(delta: number): void {
    const shifted = new Date(this.year(), this.month() - 1 + delta, 1);
    this.year.set(shifted.getFullYear());
    this.month.set(shifted.getMonth() + 1);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.calendarService.getMyMonth(this.year(), this.month()).subscribe({
      next: (month) => {
        this.data.set(month);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.data.set(null);

        const problem = err.error as { correlationId?: string } | null;
        this.error.set({
          message: 'Could not load your calendar. Please try again.',
          correlationId: problem?.correlationId ?? null,
        });
      },
    });
  }
}

function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}
