import { Component, input, output } from '@angular/core';

// The one error UI: a calm banner with a human message and, when the failure
// came from the API, the ProblemDetails correlation id — so any error on
// screen can be matched to its backend log lines.
@Component({
  selector: 'app-error-banner',
  template: `
    <div class="banner" role="alert">
      <div class="body">
        <p>{{ message() }}</p>
        @if (correlationId(); as id) {
          <p class="correlation">Support id: <code>{{ id }}</code></p>
        }
      </div>
      @if (dismissible()) {
        <button type="button" (click)="dismissed.emit()" aria-label="Dismiss error">✕</button>
      }
    </div>
  `,
  styles: `
    .banner {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.75rem;
      border: 1px solid var(--danger-border);
      background: var(--danger-bg);
      color: var(--danger-text);
      border-radius: var(--radius);
      padding: 0.75rem 1rem;
      margin-bottom: 1rem;
    }

    p {
      margin: 0;
    }

    .correlation {
      margin-top: 0.35rem;
      font-size: 0.85rem;

      code {
        background: #f6dedc;
        border-radius: 4px;
        padding: 0.1rem 0.35rem;
      }
    }

    button {
      border: none;
      background: none;
      color: inherit;
      cursor: pointer;
      line-height: 1;
      padding: 0.1rem 0.25rem;
    }
  `,
})
export class ErrorBanner {
  readonly message = input.required<string>();
  readonly correlationId = input<string | null>(null);
  readonly dismissible = input(false);
  readonly dismissed = output<void>();
}
