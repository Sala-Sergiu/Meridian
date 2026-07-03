import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthState } from '../../core/auth/auth-state';
import { ErrorBanner } from '../../shared/ui/error-banner';
import { Loading } from '../../shared/ui/loading';
import { HireProgress } from './hires.models';
import { HiresService } from './hires.service';

// HR/Manager tracking view: every new hire with their onboarding progress.
// The assign action shows for HR only — display-only sugar; the backend
// HrWrite policy is the real enforcement (a Manager POST would get 403).
@Component({
  selector: 'app-hires-page',
  imports: [ErrorBanner, Loading],
  templateUrl: './hires-page.html',
  styleUrl: './hires-page.scss',
})
export class HiresPage implements OnInit {
  private readonly hiresService = inject(HiresService);
  private readonly authState = inject(AuthState);

  protected readonly isHr = computed(() => this.authState.user()?.role === 'HR');

  protected readonly loading = signal(true);
  protected readonly error = signal<{ message: string; correlationId: string | null } | null>(null);
  protected readonly assigningId = signal<number | null>(null);
  protected readonly hires = signal<HireProgress[]>([]);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.hiresService.getProgress().subscribe({
      next: (rows) => {
        this.hires.set(rows);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        const problem = err.error as { correlationId?: string } | null;
        this.error.set({
          message: 'Could not load the hires overview. Please try again.',
          correlationId: problem?.correlationId ?? null,
        });
      },
    });
  }

  protected assign(hire: HireProgress): void {
    if (this.assigningId() !== null) {
      return;
    }

    this.assigningId.set(hire.hireUserId);
    this.hiresService.assignBoard(hire.hireUserId).subscribe({
      next: () => {
        this.assigningId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.assigningId.set(null);
        const problem = err.error as { correlationId?: string } | null;
        this.error.set({
          message: `Could not assign onboarding to ${hire.displayName}.`,
          correlationId: problem?.correlationId ?? null,
        });
      },
    });
  }

  protected percent(done: number, total: number): number {
    return total === 0 ? 0 : Math.round((done / total) * 100);
  }
}
