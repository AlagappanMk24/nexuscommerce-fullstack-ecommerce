import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="unauthorized-page">
      <div class="content animate-fadeInUp">
        <div class="error-code">403</div>
        <h1>Access Denied</h1>
        <p>You don't have permission to view this page. Please log in with the appropriate account.</p>
        <a routerLink="/auth/login" class="btn btn-primary btn-lg">Go to Login</a>
      </div>
    </div>
  `,
  styles: [`
    .unauthorized-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-primary);
      text-align: center;
      padding: 2rem;
    }
    .content {
      max-width: 480px;
    }
    .error-code {
      font-size: 7rem;
      font-weight: 900;
      background: linear-gradient(135deg, #6c63ff, #ff6584);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      line-height: 1;
      margin-bottom: 1rem;
    }
    h1 {
      font-size: 1.8rem;
      font-weight: 700;
      margin-bottom: 1rem;
    }
    p {
      color: var(--text-muted);
      font-size: 0.95rem;
      line-height: 1.6;
      margin-bottom: 2rem;
    }
  `]
})
export class UnauthorizedComponent {}
