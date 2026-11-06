import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  template: `<div class="state"><h1>404</h1><p>Страница не найдена.</p><a routerLink="/">На главную</a></div>`,
})
export class NotFound {}
