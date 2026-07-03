import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { RESOURCE_CONTENT, ResourceContent } from './resource-content';

// Simple static resource page the board cards link to. The slug comes from
// the route; unknown slugs get a friendly not-found state instead of a crash.
@Component({
  selector: 'app-resource-page',
  imports: [RouterLink],
  templateUrl: './resource-page.html',
  styleUrl: './resource-page.scss',
})
export class ResourcePage {
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);

  protected readonly resource = computed<ResourceContent | null>(() => {
    const slug = this.params()?.get('slug');
    return slug ? (RESOURCE_CONTENT[slug] ?? null) : null;
  });
}
