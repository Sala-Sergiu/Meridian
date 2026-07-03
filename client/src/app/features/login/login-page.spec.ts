import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let controller: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPage);
    controller = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    controller.verify();
    localStorage.clear();
  });

  function fillForm(email: string, password: string): void {
    const element = fixture.nativeElement as HTMLElement;
    const emailInput = element.querySelector<HTMLInputElement>('input[type="email"]')!;
    const passwordInput = element.querySelector<HTMLInputElement>('input[type="password"]')!;
    emailInput.value = email;
    emailInput.dispatchEvent(new Event('input'));
    passwordInput.value = password;
    passwordInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function submitForm(): void {
    (fixture.nativeElement as HTMLElement).querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('does not call the API while the form is invalid', () => {
    fillForm('not-an-email', '');
    submitForm();

    controller.expectNone(`${environment.apiBaseUrl}/auth/login`);
    expect((fixture.nativeElement as HTMLElement).querySelectorAll('.field-error').length)
      .toBeGreaterThan(0);
  });

  it('navigates to /board after a successful login', async () => {
    const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fillForm('newhire@meridian.local', 'NewHire#123');
    submitForm();

    controller.expectOne(`${environment.apiBaseUrl}/auth/login`).flush({
      token: 'jwt-abc',
      user: { id: 1, email: 'newhire@meridian.local', displayName: 'Nadia NewHire', role: 'NewHire' },
    });

    expect(navigate).toHaveBeenCalledWith('/board');
  });

  it('lands HR on the hires tracking page', async () => {
    const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fillForm('hr@meridian.local', 'HrAdmin#123');
    submitForm();

    controller.expectOne(`${environment.apiBaseUrl}/auth/login`).flush({
      token: 'jwt-hr',
      user: { id: 2, email: 'hr@meridian.local', displayName: 'Hannah HR', role: 'HR' },
    });

    expect(navigate).toHaveBeenCalledWith('/hires');
  });

  it('shows the generic error message when the login fails', () => {
    fillForm('newhire@meridian.local', 'wrong');
    submitForm();

    controller
      .expectOne(`${environment.apiBaseUrl}/auth/login`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });
    fixture.detectChanges();

    const error = (fixture.nativeElement as HTMLElement).querySelector('.form-error');
    expect(error?.textContent).toContain('Invalid email or password.');
  });
});
