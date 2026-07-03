import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';
import { AuthState } from './core/auth/auth-state';

describe('App', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  afterEach(() => localStorage.clear());

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the wordmark', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Meridian');
  });

  it('hides user info while logged out', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('.whoami')).toBeNull();
  });

  it('shows name, role, and a logout that clears the session and goes to /login', async () => {
    TestBed.inject(AuthState).setSession({
      token: 'jwt-abc',
      user: { id: 1, email: 'newhire@meridian.local', displayName: 'Nadia NewHire', role: 'NewHire' },
    });
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.whoami .name')?.textContent).toContain('Nadia NewHire');
    expect(element.querySelector('.whoami .role')?.textContent).toContain('NewHire');

    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    element.querySelector<HTMLButtonElement>('.whoami button')!.click();

    expect(TestBed.inject(AuthState).isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith('/login');
  });
});
