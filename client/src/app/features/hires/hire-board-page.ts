import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ErrorBanner } from '../../shared/ui/error-banner';
import { Loading } from '../../shared/ui/loading';
import { BoardCard, CardStatus } from '../board/board.models';
import { HireProgress } from './hires.models';
import { HiresService } from './hires.service';

interface BoardColumn {
  status: CardStatus;
  label: string;
  cards: BoardCard[];
}

const COLUMN_DEFS: { status: CardStatus; label: string }[] = [
  { status: 'ToDo', label: 'To do' },
  { status: 'InProgress', label: 'In progress' },
  { status: 'Done', label: 'Done' },
];

const byOrder = (a: BoardCard, b: BoardCard) => a.order - b.order;

// Read-only mirror of one hire's board for HR/Manager — the same layout the
// hire sees (required reading + task columns), minus every write affordance:
// no drag & drop, no acknowledge. The hire's name comes from the progress
// list; the backend HrOrManagerRead policy guards the board endpoint.
@Component({
  selector: 'app-hire-board-page',
  imports: [ErrorBanner, Loading, RouterLink],
  templateUrl: './hire-board-page.html',
  styleUrl: './hire-board-page.scss',
})
export class HireBoardPage implements OnInit {
  private readonly hiresService = inject(HiresService);

  protected readonly hireUserId = Number(
    inject(ActivatedRoute).snapshot.paramMap.get('hireUserId'),
  );

  protected readonly loading = signal(true);
  protected readonly noBoard = signal(false);
  protected readonly error = signal<{ message: string; correlationId: string | null } | null>(null);
  protected readonly hire = signal<HireProgress | null>(null);
  private readonly cards = signal<BoardCard[]>([]);

  protected readonly attention = computed(() =>
    this.cards().filter((c) => c.type === 'Safety').sort(byOrder),
  );
  private readonly tasks = computed(() => this.cards().filter((c) => c.type === 'Resource'));

  protected readonly columns = computed<BoardColumn[]>(() =>
    COLUMN_DEFS.map((def) => ({
      ...def,
      cards: this.tasks().filter((card) => card.status === def.status).sort(byOrder),
    })),
  );

  protected readonly attentionRead = computed(
    () => this.attention().filter((c) => c.status === 'Done').length,
  );
  protected readonly tasksDone = computed(
    () => this.tasks().filter((c) => c.status === 'Done').length,
  );
  protected readonly tasksTotal = computed(() => this.tasks().length);
  protected readonly allDone = computed(
    () =>
      this.tasksTotal() > 0 &&
      this.tasksDone() === this.tasksTotal() &&
      this.attentionRead() === this.attention().length,
  );

  ngOnInit(): void {
    forkJoin({
      board: this.hiresService.getBoard(this.hireUserId),
      progress: this.hiresService.getProgress(),
    }).subscribe({
      next: ({ board, progress }) => {
        this.cards.set(board.cards);
        this.hire.set(progress.find((h) => h.hireUserId === this.hireUserId) ?? null);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);

        // 404 means "no board assigned yet" — an expected state, not an error.
        if (err.status === 404) {
          this.noBoard.set(true);
          return;
        }

        const problem = err.error as { correlationId?: string } | null;
        this.error.set({
          message: 'Could not load this board. Please try again.',
          correlationId: problem?.correlationId ?? null,
        });
      },
    });
  }
}
