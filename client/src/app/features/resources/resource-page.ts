import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BoardService } from '../board/board.service';
import { BoardCard } from '../board/board.models';
import { RESOURCE_CONTENT, ResourceContent } from './resource-content';

// Static resource page the board cards link to. The page also owns the
// acknowledgment flow: opening a task counts as starting it (ToDo →
// InProgress), and the mark-as-read/done button sits at the END of the
// article — you confirm after reading, not from the board. Contacts and
// visitors without a board just get the content, no buttons.
@Component({
  selector: 'app-resource-page',
  imports: [RouterLink],
  templateUrl: './resource-page.html',
  styleUrl: './resource-page.scss',
})
export class ResourcePage implements OnInit {
  private readonly boardService = inject(BoardService);
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);

  protected readonly saving = signal(false);
  private readonly boardCards = signal<BoardCard[]>([]);
  private readonly startedCardIds = new Set<number>();

  protected readonly resource = computed<ResourceContent | null>(() => {
    const slug = this.params()?.get('slug');
    return slug ? (RESOURCE_CONTENT[slug] ?? null) : null;
  });

  // The hire's own board card behind this page, matched by url. Null when
  // the visitor has no board or no card links here.
  protected readonly card = computed<BoardCard | null>(() => {
    const slug = this.params()?.get('slug');
    return this.boardCards().find((c) => c.url === `/resources/${slug}`) ?? null;
  });

  // Only required reading (Safety) and tasks (Resource) are acknowledged;
  // contact pages are pure reference.
  protected readonly acknowledgeable = computed(() => {
    const card = this.card();
    return card !== null && (card.type === 'Safety' || card.type === 'Resource');
  });

  protected readonly done = computed(() => this.card()?.status === 'Done');

  protected readonly buttonLabel = computed(() =>
    this.card()?.type === 'Safety' ? 'Mark as read' : 'Mark as done',
  );

  protected readonly doneLabel = computed(() =>
    this.card()?.type === 'Safety' ? '✓ Read' : '✓ Done',
  );

  ngOnInit(): void {
    this.boardService.getMyBoard().subscribe({
      next: (page) => {
        this.boardCards.set(page.items);
        this.autoStart();
      },
      // No board (404) or any failure: the article still renders, just
      // without the acknowledgment flow.
      error: () => this.boardCards.set([]),
    });
  }

  protected acknowledge(): void {
    const card = this.card();
    if (card === null || this.saving() || card.status === 'Done') {
      return;
    }

    this.saving.set(true);
    this.boardService.moveCard(card.id, 'Done').subscribe({
      next: (updated) => {
        this.replaceCard(updated);
        this.saving.set(false);
      },
      error: () => this.saving.set(false),
    });
  }

  // Opening a task means you started it: ToDo → InProgress, fire-and-forget.
  // Safety cards stay as-is until explicitly marked read.
  private autoStart(): void {
    const card = this.card();
    if (
      card === null ||
      card.type !== 'Resource' ||
      card.status !== 'ToDo' ||
      this.startedCardIds.has(card.id)
    ) {
      return;
    }

    this.startedCardIds.add(card.id);
    this.boardService.moveCard(card.id, 'InProgress').subscribe({
      next: (updated) => this.replaceCard(updated),
      error: () => {
        // Purely a convenience transition — reading on regardless.
      },
    });
  }

  private replaceCard(updated: BoardCard): void {
    this.boardCards.update((cards) => cards.map((c) => (c.id === updated.id ? updated : c)));
  }
}
