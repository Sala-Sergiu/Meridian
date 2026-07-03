import { AuthState } from './auth-state';
import { LoginResult } from './auth.models';

const SESSION: LoginResult = {
  token: 'jwt-abc',
  user: { id: 1, email: 'newhire@meridian.local', displayName: 'Nadia NewHire', role: 'NewHire' },
};

// Instantiated directly (no TestBed needed): the store has no dependencies,
// and construction is exactly the "page refresh" moment we want to test.
describe('AuthState', () => {
  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('starts logged out with empty storage', () => {
    const state = new AuthState();

    expect(state.isAuthenticated()).toBe(false);
    expect(state.token()).toBeNull();
    expect(state.user()).toBeNull();
  });

  it('exposes token and user after setSession and persists them', () => {
    const state = new AuthState();

    state.setSession(SESSION);

    expect(state.isAuthenticated()).toBe(true);
    expect(state.token()).toBe('jwt-abc');
    expect(state.user()?.displayName).toBe('Nadia NewHire');
    expect(localStorage.getItem('meridian.session')).not.toBeNull();
  });

  it('restores the session from storage on construction (page refresh)', () => {
    new AuthState().setSession(SESSION);

    const refreshed = new AuthState();

    expect(refreshed.isAuthenticated()).toBe(true);
    expect(refreshed.token()).toBe('jwt-abc');
    expect(refreshed.user()?.role).toBe('NewHire');
  });

  it('clear logs out and removes the persisted session', () => {
    const state = new AuthState();
    state.setSession(SESSION);

    state.clear();

    expect(state.isAuthenticated()).toBe(false);
    expect(new AuthState().isAuthenticated()).toBe(false);
    expect(localStorage.getItem('meridian.session')).toBeNull();
  });

  it('treats corrupt stored data as logged out', () => {
    localStorage.setItem('meridian.session', 'not-json{');

    expect(new AuthState().isAuthenticated()).toBe(false);
  });
});
