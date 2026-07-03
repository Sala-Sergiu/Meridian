import { Component, input } from '@angular/core';

// The one loading indicator: a small spinner plus a message. Fixed height so
// swapping it for content doesn't shift the layout.
@Component({
  selector: 'app-loading',
  template: `
    <div class="loading" role="status">
      <span class="spinner" aria-hidden="true"></span>
      <span>{{ message() }}</span>
    </div>
  `,
  styles: `
    .loading {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      min-height: 3rem;
      color: var(--text-muted);
    }

    .spinner {
      width: 1rem;
      height: 1rem;
      border: 2px solid var(--border);
      border-top-color: var(--accent);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }
  `,
})
export class Loading {
  readonly message = input('Loading…');
}
