import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthState } from './core/auth/auth-state';
import { AuthService } from './core/auth/auth.service';

// App shell: the Meridian wordmark plus, when logged in, who you are and a
// logout — shared by every screen. The role chip is display-only; the backend
// owns what a role may actually do.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user = inject(AuthState).user;

  protected logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
