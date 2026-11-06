import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { AuthResponse } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private state = signal<AuthResponse | null>(this.load());

  readonly token = computed(() => this.state()?.token ?? null);
  readonly userId = computed(() => this.state()?.userId ?? null);
  readonly userName = computed(() => this.state()?.userName ?? null);
  readonly isLoggedIn = computed(() => this.state() !== null);

  login(body: { userName: string; password: string }) {
    return this.http.post<AuthResponse>('/api/auth/login', body).pipe(tap(r => this.save(r)));
  }

  register(body: { userName: string; password: string }) {
    return this.http.post<AuthResponse>('/api/auth/register', body).pipe(tap(r => this.save(r)));
  }

  logout() {
    localStorage.removeItem('auth');
    this.state.set(null);
  }

  private save(r: AuthResponse) {
    localStorage.setItem('auth', JSON.stringify(r));
    this.state.set(r);
  }

  private load(): AuthResponse | null {
    try { return JSON.parse(localStorage.getItem('auth') ?? 'null'); } catch { return null; }
  }
}
