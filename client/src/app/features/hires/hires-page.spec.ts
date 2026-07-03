import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthState } from '../../core/auth/auth-state';
import { Role } from '../../core/auth/auth.models';
import { HireProgress } from './hires.models';
import { HiresPage } from './hires-page';

function hire(partial: Partial<HireProgress> & { hireUserId: number }): HireProgress {
  return {
    displayName: `Hire ${partial.hireUserId}`,
    email: `hire${partial.hireUserId}@meridian.local`,
    hasBoard: true,
    tasksDone: 0,
    tasksTotal: 2,
    readDone: 0,
    readTotal: 2,
    ...partial,
  };
}

describe('HiresPage', () => {
  let fixture: ComponentFixture<HiresPage>;
  let controller: HttpTestingController;

  async function render(role: Role, rows: HireProgress[]): Promise<HTMLElement> {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [HiresPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    TestBed.inject(AuthState).setSession({
      token: 'jwt',
      user: { id: 99, email: 'x@meridian.local', displayName: 'X', role },
    });
    controller = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(HiresPage);
    fixture.detectChanges();

    controller.expectOne(`${environment.apiBaseUrl}/boards/progress`).flush(rows);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  afterEach(() => {
    controller.verify();
    localStorage.clear();
  });

  it('lists hires with reading and task progress', async () => {
    const element = await render('Manager', [
      hire({ hireUserId: 1, displayName: 'Nadia', tasksDone: 1, tasksTotal: 2, readDone: 2, readTotal: 2 }),
    ]);

    const row = element.querySelector('tbody tr')!;
    expect(row.textContent).toContain('Nadia');
    expect(row.textContent).toContain('2/2');
    expect(row.textContent).toContain('1/2');
  });

  it('shows the assign action to HR for hires without a board', async () => {
    const element = await render('HR', [hire({ hireUserId: 4, hasBoard: false })]);

    expect(element.textContent).toContain('No onboarding assigned yet');
    const button = element.querySelector<HTMLButtonElement>('tbody button')!;
    expect(button.textContent).toContain('Assign onboarding');

    button.click();
    fixture.detectChanges();

    const req = controller.expectOne(`${environment.apiBaseUrl}/boards/assign`);
    expect(req.request.body).toEqual({ templateId: 1, hireUserId: 4 });
    req.flush({});

    // The list reloads after a successful assignment.
    controller
      .expectOne(`${environment.apiBaseUrl}/boards/progress`)
      .flush([hire({ hireUserId: 4, hasBoard: true })]);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('No onboarding assigned yet');
  });

  it('hides the assign action from managers (display-only; backend enforces)', async () => {
    const element = await render('Manager', [hire({ hireUserId: 4, hasBoard: false })]);

    expect(element.textContent).toContain('No onboarding assigned yet');
    expect(element.querySelector('tbody button')).toBeNull();
  });

  it('lets HR publish an article to the template and all boards', async () => {
    const element = await render('HR', [hire({ hireUserId: 1 })]);

    const title = element.querySelector<HTMLInputElement>('.publish input[formcontrolname="title"]')!;
    const description = element.querySelector<HTMLTextAreaElement>('.publish textarea')!;
    title.value = 'GDPR refresher';
    title.dispatchEvent(new Event('input'));
    description.value = 'Annual privacy training.';
    description.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    const req = controller.expectOne(`${environment.apiBaseUrl}/templates/1/cards`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      title: 'GDPR refresher',
      description: 'Annual privacy training.',
      type: 'Resource',
      url: null,
    });
    req.flush({ card: { id: 9, title: 'GDPR refresher' }, boardsUpdated: 3 });
    fixture.detectChanges();

    // The overview reloads so the new totals show up.
    controller
      .expectOne(`${environment.apiBaseUrl}/boards/progress`)
      .flush([hire({ hireUserId: 1, tasksTotal: 3 })]);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('3 existing board(s)');
    expect(text).toContain('0/3');
  });

  it('hides the publish form from managers', async () => {
    const element = await render('Manager', [hire({ hireUserId: 1 })]);

    expect(element.querySelector('.publish')).toBeNull();
  });
});
