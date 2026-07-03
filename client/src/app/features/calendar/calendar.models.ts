// Frontend mirrors of the backend calendar contract (CalendarMonthDto).

export type DayKind = 'Office' | 'Remote' | 'Holiday' | 'Weekend';

export interface CalendarDay {
  date: string; // ISO yyyy-MM-dd
  kind: DayKind;
  holidayName: string | null;
}

export interface CalendarMonth {
  year: number;
  month: number;
  days: CalendarDay[];
}
