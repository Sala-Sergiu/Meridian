// Frontend mirrors of the backend auth contract (LoginRequestDto /
// LoginResultDto / UserDto), matched to the actual Swagger shapes: the user
// fields arrive NESTED under `user`, not flat.
//
// SECURITY NOTE: the role received here is for UX only (show/hide UI). It is
// NOT a security boundary — the backend authorization policies (owner-only,
// HR-only) are the real enforcement. Never rely on the client-side role for
// protection.

export type Role = 'NewHire' | 'HR' | 'Manager';

export interface AuthUser {
  id: number;
  email: string;
  displayName: string;
  role: Role;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// Also the shape persisted as the session: token + the user it belongs to.
export interface LoginResult {
  token: string;
  user: AuthUser;
}
