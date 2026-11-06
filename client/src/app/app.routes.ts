import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { JobDetail } from './job-detail';
import { JobForm } from './job-form';
import { JobList } from './job-list';
import { Login } from './login';
import { NotFound } from './not-found';
import { Register } from './register';

export const routes: Routes = [
  { path: '', component: JobList },
  { path: 'jobs/new', component: JobForm, canActivate: [authGuard] },
  { path: 'jobs/:id/edit', component: JobForm, canActivate: [authGuard] },
  { path: 'jobs/:id', component: JobDetail },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: '**', component: NotFound },
];
