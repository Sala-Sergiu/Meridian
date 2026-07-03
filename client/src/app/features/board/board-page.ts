import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
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

// Kanban over the hire's own board: three status columns, cards sorted by
// their template order. Dragging a card to another column persists the move —
// the only write on the board. No role checks here: the backend owns
// authorization; the frontend just attempts the move.
@Component({
  selector: 'app-board-page',
  imports: [CdkDropListGroup, CdkDropList, CdkDrag],
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
  private readonly cards = signal<BoardCard[]>([]);

  // One computed buckets the flat card list into the three columns; the
  // backend already sorts by order, the sort here just makes it a local
  // invariant instead of a remote one.
  protected readonly columns = computed<BoardColumn[]>(() =>
    COLUMN_DEFS.map((def) => ({
      ...def,
      cards: this.cards()
        .filter((card) => card.status === def.status)
        .sort((a, b) => a.order - b.order),
    })),
  );

  ngOnInit(): void {
    this.load();
  }

  // Dropping into the same column is a no-op (cards keep their template
  // order); dropping into another column moves the card there.
  protected onDrop(event: CdkDragDrop<CardStatus>): void {
    if (event.previousContainer === event.container) {
      return;
    }

    this.move(event.item.data as BoardCard, event.container.data);
  }

  protected dismissMoveError(): void {
    this.moveError.set(null);
  }

  // Optimistic: the card jumps columns immediately; the PATCH runs after. On
  // success the server DTO is taken as truth; on failure the card is put back
  // where it was and the ProblemDetails correlation id is surfaced.
  private move(card: BoardCard, newStatus: CardStatus): void {
    const previousStatus = card.status;
    this.setCardStatus(card.id, newStatus);
    this.moveError.set(null);

    this.boardService.moveCard(card.id, newStatus).subscribe({
      next: (updated) => this.replaceCard(updated),
      error: (err: HttpErrorResponse) => {
        this.setCardStatus(card.id, previousStatus);

        const problem = err.error as { correlationId?: string } | null;
        this.moveError.set({
          message: `Could not move "${card.title}" — it has been put back.`,
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
}
