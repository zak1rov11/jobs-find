import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from './auth.service';
import { applyServerErrors, controlError } from './form-errors';

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1>Регистрация</h1>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>Логин <input formControlName="userName"></label>
      <small class="err">{{ err('userName') }}</small>
      <label>Пароль <input type="password" formControlName="password"></label>
      <small class="err">{{ err('password') }}</small>
      @if (error()) { <p class="err">{{ error() }}</p> }
      <button type="submit" [disabled]="loading()">Зарегистрироваться</button>
      <small>Уже есть аккаунт? <a routerLink="/login">Вход</a></small>
    </form>
  `,
})
export class Register {
  private fb = inject(NonNullableFormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    userName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(30)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });
  error = signal('');
  loading = signal(false);

  err(n: string) { return controlError(this.form.get(n)); }

  submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/']),
      error: (e: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(applyServerErrors(this.form, e));
      },
    });
  }
}
