export interface Sphere { id: number; name: string; }

export interface Job {
  id: number; title: string; company: string; salaryFrom: number; salaryTo: number;
  isRemote: boolean; logoUrl: string | null; sphereId: number; sphereName: string;
  ownerId: number; ownerName: string; createdAt: string;
}

export interface JobRequest {
  title: string; company: string; salaryFrom: number; salaryTo: number;
  isRemote: boolean; sphereId: number;
}

export interface Paged<T> { items: T[]; page: number; pageSize: number; total: number; }

export interface Filters {
  sphereId: number | null; remote: boolean; minSalary: number | null; sort: 'newest' | 'salary';
}

export interface AuthResponse { token: string; userId: number; userName: string; }

export function money(n: number): string {
  return n.toLocaleString('ru-RU');
}
