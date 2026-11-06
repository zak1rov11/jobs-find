import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Filters, Sphere } from './models';

@Component({
  selector: 'app-job-filter',
  imports: [FormsModule],
  template: `
    <div class="filter">
      <label>Сфера
        <select [(ngModel)]="sphereId">
          <option [ngValue]="null">Все</option>
          @for (s of spheres(); track s.id) { <option [ngValue]="s.id">{{ s.name }}</option> }
        </select>
      </label>
      <label>Зарплата от
        <input type="number" min="0" [(ngModel)]="minSalary">
      </label>
      <label>Сортировка
        <select [(ngModel)]="sort">
          <option value="newest">Сначала новые</option>
          <option value="salary">По зарплате</option>
        </select>
      </label>
      <label><span><input type="checkbox" [(ngModel)]="remote"> Только удалённо</span></label>
      <button (click)="apply()">Применить</button>
      <button class="sec" (click)="reset()">Сбросить</button>
    </div>
  `,
})
export class JobFilter {
  spheres = input.required<Sphere[]>();
  changed = output<Filters>();

  sphereId = signal<number | null>(null);
  minSalary = signal<number | null>(null);
  remote = signal(false);
  sort = signal<'newest' | 'salary'>('newest');

  apply() {
    this.changed.emit({
      sphereId: this.sphereId(), minSalary: this.minSalary(),
      remote: this.remote(), sort: this.sort(),
    });
  }

  reset() {
    this.sphereId.set(null); this.minSalary.set(null);
    this.remote.set(false); this.sort.set('newest');
    this.apply();
  }
}
