import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
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
  imports: [ErrorBanner, Loading, ReactiveFormsModule, RouterLink],
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

  // --- HR publishing: a new article goes onto the template AND onto every
  // existing board (backend broadcast). Form shows for HR only.

  private readonly formBuilder = inject(NonNullableFormBuilder);

  protected readonly publishing = signal(false);
  protected readonly publishSuccess = signal<string | null>(null);
  protected readonly publishError = signal<{ message: string; correlationId: string | null } | null>(
    null,
  );

  protected readonly publishForm = this.formBuilder.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
    type: this.formBuilder.control<'Safety' | 'Resource'>('Resource'),
    url: [''],
  });

  protected publish(): void {
    if (this.publishForm.invalid) {
      this.publishForm.markAllAsTouched();
      return;
    }

    this.publishing.set(true);
    this.publishSuccess.set(null);
    this.publishError.set(null);

    const value = this.publishForm.getRawValue();
    this.hiresService
      .publishCard({
        title: value.title,
        description: value.description,
        type: value.type,
        url: value.url.trim() === '' ? null : value.url.trim(),
      })
      .subscribe({
        next: (result) => {
          this.publishing.set(false);
          this.publishSuccess.set(
            `"${value.title}" published — added to the template and to ${result.boardsUpdated} existing board(s).`,
          );
          this.publishForm.reset({ title: '', description: '', type: 'Resource', url: '' });
          this.load();
        },
        error: (err: HttpErrorResponse) => {
          this.publishing.set(false);
          const problem = err.error as { correlationId?: string } | null;
          this.publishError.set({
            message: 'Could not publish the article.',
            correlationId: problem?.correlationId ?? null,
          });
        },
      });
  }
}
