import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { applyServerErrors, controlError } from './form-errors';
import { Job, JobRequest, Sphere } from './models';
import { JobsService } from './jobs.service';

/** ОСОБОЕ ПРАВИЛО на клиенте: «зарплата от» не больше «зарплата до». */
function salaryRange(c: AbstractControl): ValidationErrors | null {
  const from = c.parent?.get('salaryFrom')?.value;
  const to = c.value;
  return typeof from === 'number' && typeof to === 'number' && to < from ? { salaryRange: true } : null;
}

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <a routerLink="/">← К списку</a>
    <h1>{{ id() ? 'Редактирование' : 'Новая вакансия' }}</h1>

    @if (spheres.isLoading() || job.isLoading()) {
      <p class="state">Загрузка…</p>
    } @else if (spheres.error() || job.error()) {
      <p class="state error">Не удалось загрузить данные.</p>
    } @else if (spheres.value()?.length === 0) {
      <p class="state">Справочник сфер пуст.</p>
    } @else {
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>Должность <input formControlName="title"></label>
        <small class="err">{{ err('title') }}</small>

        <label>Компания <input formControlName="company"></label>
        <small class="err">{{ err('company') }}</small>

        <label>Зарплата от <input type="number" formControlName="salaryFrom"></label>
        <small class="err">{{ err('salaryFrom') }}</small>

        <label>Зарплата до <input type="number" formControlName="salaryTo"></label>
        <small class="err">{{ err('salaryTo') }}</small>

        <label>Сфера
          <select formControlName="sphereId">
            <option [ngValue]="null" disabled>— выберите —</option>
            @for (s of spheres.value(); track s.id) { <option [ngValue]="s.id">{{ s.name }}</option> }
          </select>
        </label>
        <small class="err">{{ err('sphereId') }}</small>

        <label><span><input type="checkbox" formControlName="isRemote"> Удалённая работа</span></label>

        @if (formError()) { <p class="err">{{ formError() }}</p> }
        <button type="submit" [disabled]="saving()">{{ saving() ? 'Сохранение…' : 'Сохранить' }}</button>
      </form>
    }
  `,
})
export class JobForm {
  id = input<string>();
  private fb = inject(NonNullableFormBuilder);
  private api = inject(JobsService);
  private router = inject(Router);

  spheres = httpResource<Sphere[]>(() => '/api/spheres');
  job = httpResource<Job>(() => (this.id() ? `/api/jobs/${this.id()}` : undefined));

  form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    company: ['', [Validators.required, Validators.maxLength(100)]],
    salaryFrom: [0, [Validators.required, Validators.min(0), Validators.max(1000000000)]],
    salaryTo: [0, [Validators.required, Validators.min(0), Validators.max(1000000000), salaryRange]],
    isRemote: [false],
    sphereId: [null as number | null, Validators.required],
  });

  saving = signal(false);
  formError = signal('');

  constructor() {
    // при изменении «от» заново проверяем «до»
    this.form.controls.salaryFrom.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.salaryTo.updateValueAndValidity());

    effect(() => {
      const j = this.job.value();
      if (j) this.form.patchValue({
        title: j.title, company: j.company, salaryFrom: j.salaryFrom, salaryTo: j.salaryTo,
        isRemote: j.isRemote, sphereId: j.sphereId,
      });
    });
  }

  err(name: string) { return controlError(this.form.get(name)); }

  submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving.set(true);
    this.formError.set('');

    const body = this.form.getRawValue() as JobRequest;
    const id = this.id();
    const req = id ? this.api.update(+id, body) : this.api.create(body);
    req.subscribe({
      next: j => this.router.navigate(['/jobs', j.id]),
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, e));
      },
    });
  }
}
