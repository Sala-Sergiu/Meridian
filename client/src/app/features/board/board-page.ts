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

// Read-only Kanban over the hire's own board: three status columns, cards
// sorted by their template order. Card moves land in the next slice.
@Component({
  selector: 'app-board-page',
  templateUrl: './board-page.html',
  styleUrl: './board-page.scss',
})
export class BoardPage implements OnInit {
  private readonly boardService = inject(BoardService);

  protected readonly loading = signal(true);
  protected readonly noBoard = signal(false);
  protected readonly error = signal<{ message: string; correlationId: string | null } | null>(null);
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
