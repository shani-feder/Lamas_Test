import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <nav class="navbar">
      <div class="brand">מערכת בקרות נתונים</div>
      <div class="nav-links">
        <a routerLink="/controls">בקרות</a>
        <a routerLink="/ai-suggestions">🤖 הצעות AI</a>
      </div>
    </nav>
    <main>
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .navbar { background: #1976d2; color: white; padding: 15px 30px; display: flex; justify-content: space-between; align-items: center; direction: rtl; }
    .brand { font-size: 20px; font-weight: bold; }
    .nav-links a { color: white; text-decoration: none; margin-left: 20px; padding: 8px 16px; border-radius: 4px; transition: background 0.2s; }
    .nav-links a:hover { background: rgba(255,255,255,0.1); }
    main { padding: 20px; }
  `]
})
export class AppComponent {}
