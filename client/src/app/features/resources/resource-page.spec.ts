import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ResourcePage } from './resource-page';

async function render(slug: string) {
  await TestBed.configureTestingModule({
    imports: [ResourcePage],
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug })) } },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(ResourcePage);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('ResourcePage', () => {
  it('renders the content for a known slug', async () => {
    const element = await render('safety-basics');

    expect(element.querySelector('h2')?.textContent).toBe('Workplace safety basics');
    expect(element.textContent).toContain('Evacuation routes');
    expect(element.querySelector('a.back')?.getAttribute('href')).toBe('/board');
  });

  it('shows a friendly not-found state for an unknown slug', async () => {
    const element = await render('no-such-resource');

    expect(element.querySelector('h2')?.textContent).toBe('Resource not found');
    expect(element.querySelector('a.back')).not.toBeNull();
  });
});
