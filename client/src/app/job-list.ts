import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Filters, Job, Paged, Sphere } from './models';
import { JobCard } from './job-card';
import { JobFilter } from './job-filter';

@Component({
  imports: [JobCard, JobFilter],
  template: `
    <h1>Вакансии</h1>

    @if (spheres.hasValue()) {
      <app-job-filter [spheres]="spheres.value()" (changed)="apply($event)" />
    }

    @let data = jobs.value();
    @if (jobs.isLoading()) {
      <p class="state">Загрузка…</p>
    } @else if (jobs.error()) {
      <p class="state error">Не удалось загрузить список.
        <button (click)="jobs.reload()">Повторить</button></p>
    } @else if (data) {
      @if (data.items.length === 0) {
        <p class="state">Ничего не найдено. Измените фильтры.</p>
      } @else {
        <div class="grid">
          @for (j of data.items; track j.id) {
            <app-job-card [job]="j" (open)="go($event)" />
          }
        </div>
        <div class="pager">
          <button class="sec" [disabled]="page() <= 1" (click)="page.set(page() - 1)">Назад</button>
          <span>Страница {{ page() }} из {{ pages() }} · всего {{ data.total }}</span>
          <button class="sec" [disabled]="page() >= pages()" (click)="page.set(page() + 1)">Вперёд</button>
        </div>
      }
    }
  `,
})
export class JobList {
  private router = inject(Router);
  readonly pageSize = 6;

  page = signal(1);
  filters = signal<Filters>({ sphereId: null, remote: false, minSalary: null, sort: 'newest' });

  private params = computed(() => {
    const f = this.filters();
    const p: Record<string, string | number | boolean> = {
      page: this.page(), pageSize: this.pageSize, sort: f.sort,
    };
    if (f.sphereId !== null) p['sphereId'] = f.sphereId;
    if (f.remote) p['remote'] = true;
    if (f.minSalary !== null) p['minSalary'] = f.minSalary;
    return p;
  });

  spheres = httpResource<Sphere[]>(() => '/api/spheres');
  jobs = httpResource<Paged<Job>>(() => ({ url: '/api/jobs', params: this.params() }));

  pages = computed(() => Math.max(1, Math.ceil((this.jobs.value()?.total ?? 0) / this.pageSize)));

  apply(f: Filters) { this.filters.set(f); this.page.set(1); }
  go(id: number) { this.router.navigate(['/jobs', id]); }
}
