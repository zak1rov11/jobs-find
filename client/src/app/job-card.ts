import { Component, input, output } from '@angular/core';
import { Job, money } from './models';

@Component({
  selector: 'app-job-card',
  template: `
    <article class="card" (click)="open.emit(job().id)">
      @if (job().logoUrl) {
        <img [src]="job().logoUrl" [alt]="job().company">
      } @else {
        <div class="noimg">💼</div>
      }
      <h3>{{ job().title }}</h3>
      <p>{{ job().company }} · {{ job().sphereName }}</p>
      <p>{{ money(job().salaryFrom) }} – {{ money(job().salaryTo) }}</p>
      @if (job().isRemote) { <span class="badge remote">Удалённо</span> }
    </article>
  `,
})
export class JobCard {
  job = input.required<Job>();
  open = output<number>();
  money = money;
}
