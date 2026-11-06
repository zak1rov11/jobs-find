import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from './auth.service';
import { Job, money } from './models';
import { JobsService } from './jobs.service';

@Component({
  imports: [RouterLink],
  template: `
    <a routerLink="/">← К списку</a>

    @let j = job.value();
    @if (job.isLoading()) {
      <p class="state">Загрузка…</p>
    } @else if (notFound()) {
      <p class="state">Вакансия не найдена.</p>
    } @else if (job.error()) {
      <p class="state error">Ошибка загрузки. <button (click)="job.reload()">Повторить</button></p>
    } @else if (j) {
      <div class="detail">
        <h1>{{ j.title }}</h1>
        @if (j.logoUrl) { <img [src]="j.logoUrl" [alt]="j.company"> } @else { <div class="noimg">💼</div> }
        <p><b>{{ j.company }}</b> · {{ j.sphereName }}</p>
        <p>Зарплата: {{ money(j.salaryFrom) }} – {{ money(j.salaryTo) }}</p>
        <p>Формат: {{ j.isRemote ? 'удалённо' : 'в офисе' }}</p>
        <p><small>Автор: {{ j.ownerName }}</small></p>

        @if (isAuthor()) {
          <div class="row">
            <a class="btn" [routerLink]="['/jobs', j.id, 'edit']">Редактировать</a>
            <input type="file" accept="image/jpeg,image/png,image/webp" (change)="upload($event)">
            <button class="danger" (click)="remove()">Удалить</button>
          </div>
        }
        @if (message()) { <p class="err">{{ message() }}</p> }
      </div>
    } @else {
      <p class="state">Нет данных.</p>
    }
  `,
})
export class JobDetail {
  id = input.required<string>();
  private api = inject(JobsService);
  private auth = inject(AuthService);
  private router = inject(Router);
  money = money;

  job = httpResource<Job>(() => `/api/jobs/${this.id()}`);
  message = signal('');

  isAuthor = computed(() => {
    const j = this.job.value();
    return !!j && j.ownerId === this.auth.userId();
  });
  notFound = computed(() => (this.job.error() as HttpErrorResponse | undefined)?.status === 404);

  remove() {
    if (!confirm('Удалить вакансию?')) return;
    this.api.remove(+this.id()).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => this.message.set('Не удалось удалить'),
    });
  }

  upload(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.message.set('');
    this.api.uploadLogo(+this.id(), file).subscribe({
      next: () => { this.job.reload(); input.value = ''; },
      error: (e: HttpErrorResponse) => {
        this.message.set(e.error?.errors?.file?.[0] ?? e.error?.detail ?? 'Не удалось загрузить');
        input.value = '';
      },
    });
  }
}
