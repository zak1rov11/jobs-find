import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Job, JobRequest } from './models';

/** Запись данных. Чтение списков и карточек идёт через httpResource в компонентах. */
@Injectable({ providedIn: 'root' })
export class JobsService {
  private http = inject(HttpClient);

  create(body: JobRequest) { return this.http.post<Job>('/api/jobs', body); }
  update(id: number, body: JobRequest) { return this.http.put<Job>(`/api/jobs/${id}`, body); }
  remove(id: number) { return this.http.delete<void>(`/api/jobs/${id}`); }

  uploadLogo(id: number, file: File) {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<Job>(`/api/jobs/${id}/logo`, fd);
  }
}
