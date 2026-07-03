import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { ErrorBanner } from '../../shared/ui/error-banner';
import { Loading } from '../../shared/ui/loading';
import { BoardService } from './board.service';
import { BoardCard, CardStatus } from './board.models';

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

// The hire's onboarding board, split by what each card IS:
//  - Safety cards are required reading ("requires attention") — acknowledged
//    with a mark-as-read, not dragged. Same status mechanism underneath.
//  - Resource cards are actual tasks — the Kanban with drag & drop.
//  - Contact cards are reference info — always visible, no status at all.
// No role checks here: the backend owns authorization.
@Component({
  selector: 'app-board-page',
  imports: [CdkDropListGroup, CdkDropList, CdkDrag, ErrorBanner, Loading, RouterLink],
  templateUrl: './board-page.html',
  styleUrl: './board-page.scss',
})
export class BoardPage implements OnInit {
  private readonly boardService = inject(BoardService);

  protected readonly loading = signal(true);
  protected readonly noBoard = signal(false);
  protected readonly error = signal<{ message: string; correlationId: string | null } | null>(null);
  protected readonly moveError = signal<{ message: string; correlationId: string | null } | null>(
    null,
  );
  // Card whose PATCH is in flight — shown as "saving" and not actionable
  // again until the request settles.
  protected readonly pendingCardId = signal<number | null>(null);
  private readonly cards = signal<BoardCard[]>([]);

  protected readonly attention = computed(() =>
    this.cards().filter((c) => c.type === 'Safety').sort(byOrder),
  );
  protected readonly contacts = computed(() =>
    this.cards().filter((c) => c.type === 'Contact').sort(byOrder),
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
    this.load();
  }

  protected markAsRead(card: BoardCard): void {
    if (card.status !== 'Done') {
      this.move(card, 'Done');
    }
  }

  protected dismissMoveError(): void {
    this.moveError.set(null);
  }

  // Dropping into the same column is a no-op (cards keep their template
  // order); dropping into another column moves the card there.
  protected onDrop(event: CdkDragDrop<CardStatus>): void {
    if (event.previousContainer === event.container) {
      return;
    }

    this.move(event.item.data as BoardCard, event.container.data);
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.noBoard.set(false);

    this.boardService.getMyBoard().subscribe({
      next: (page) => {
        this.cards.set(page.items);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.cards.set([]);

        // 404 means "no board assigned yet" — an expected state, not an error.
        if (err.status === 404) {
          this.noBoard.set(true);
          return;
        }

        // Surface the correlation id from the ProblemDetails body so a failed
        // request on screen can be matched to its backend log lines.
        const problem = err.error as { correlationId?: string } | null;
        this.error.set({
          message: 'Could not load your board. Please try again.',
          correlationId: problem?.correlationId ?? null,
        });
      },
    });
  }

  // Optimistic: the card changes state immediately; the PATCH runs after. On
  // success the server DTO is taken as truth; on failure the card is put back
  // where it was and the ProblemDetails correlation id is surfaced.
  private move(card: BoardCard, newStatus: CardStatus): void {
    const previousStatus = card.status;
    this.setCardStatus(card.id, newStatus);
    this.moveError.set(null);
    this.pendingCardId.set(card.id);

    this.boardService.moveCard(card.id, newStatus).subscribe({
      next: (updated) => {
        this.replaceCard(updated);
        this.pendingCardId.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.setCardStatus(card.id, previousStatus);
        this.pendingCardId.set(null);

        const problem = err.error as { correlationId?: string } | null;
        this.moveError.set({
          message: `Could not update "${card.title}" — it has been put back.`,
          correlationId: problem?.correlationId ?? null,
        });
      },
    });
  }

  private setCardStatus(cardId: number, status: CardStatus): void {
    this.cards.update((cards) => cards.map((c) => (c.id === cardId ? { ...c, status } : c)));
  }

  private replaceCard(updated: BoardCard): void {
    this.cards.update((cards) => cards.map((c) => (c.id === updated.id ? updated : c)));
  }
}
