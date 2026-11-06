import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormGroup } from '@angular/forms';

/** Текст ошибки под полем (правила те же, что на сервере). */
export function controlError(c: AbstractControl | null): string {
  if (!c || !c.touched || !c.errors) return '';
  const e = c.errors;
  if (e['server']) return e['server'];
  if (e['required']) return 'Обязательное поле';
  if (e['salaryRange']) return 'Зарплата «до» не может быть меньше зарплаты «от»';
  if (e['minlength']) return `Не короче ${e['minlength'].requiredLength} символов`;
  if (e['maxlength']) return `Не длиннее ${e['maxlength'].requiredLength} символов`;
  if (e['min']) return `Не меньше ${e['min'].min}`;
  if (e['max']) return `Не больше ${e['max'].max}`;
  return 'Неверное значение';
}

/** Раскладывает errors из ProblemDetails по полям формы. Остаток возвращает как общий текст. */
export function applyServerErrors(form: FormGroup, e: HttpErrorResponse): string {
  const errors = e.error?.errors as Record<string, string[]> | undefined;
  if (e.status === 400 && errors) {
    const general: string[] = [];
    for (const [key, msgs] of Object.entries(errors)) {
      const c = form.get(key);
      if (c) { c.setErrors({ server: msgs[0] }); c.markAsTouched(); }
      else general.push(msgs[0]);
    }
    return general.join(' ');
  }
  if (e.status === 401) return 'Нужно войти';
  if (e.status === 403) return e.error?.detail ?? 'Нет прав на это действие';
  return 'Ошибка сервера, попробуйте позже';
}
