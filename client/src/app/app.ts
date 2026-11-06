import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="nav">
      <a routerLink="/">💼 Вакансии</a>
      <span class="sp"></span>
      @if (auth.isLoggedIn()) {
        <a routerLink="/jobs/new">+ Добавить</a>
        <span>{{ auth.userName() }}</span>
        <button class="sec" (click)="logout()">Выйти</button>
      } @else {
        <a routerLink="/login">Вход</a>
        <a routerLink="/register">Регистрация</a>
      }
    </header>
    <main><router-outlet /></main>
  `,
})
export class App {
  auth = inject(AuthService);
  private router = inject(Router);
  logout() { this.auth.logout(); this.router.navigate(['/']); }
}
