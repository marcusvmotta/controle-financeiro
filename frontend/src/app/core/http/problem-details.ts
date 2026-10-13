import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormGroup } from '@angular/forms';

/** Formato de erro da API (RFC 9457), ver TRD 03-api "Erros". */
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

/** Extrai o ProblemDetails de um erro HTTP, se houver. */
export function toProblemDetails(error: unknown): ProblemDetails | null {
  if (error instanceof HttpErrorResponse && error.error && typeof error.error === 'object') {
    return error.error as ProblemDetails;
  }
  return null;
}

/** Mensagem amigável para qualquer erro HTTP. */
export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse && error.status === 0) {
    return 'Não foi possível falar com o servidor. Verifique sua conexão.';
  }

  const problem = toProblemDetails(error);
  return problem?.detail ?? problem?.title ?? 'Ocorreu um erro inesperado. Tente novamente.';
}

/**
 * Copia os erros de validação da API (400) para os campos do formulário.
 * Ex.: { errors: { password: ["A senha deve ter..."] } } → form.controls.password ganha o erro "server".
 * Devolve true se algum erro foi aplicado a um campo.
 */
export function applyServerValidationErrors(form: FormGroup, error: unknown): boolean {
  const errors = toProblemDetails(error)?.errors;
  if (!errors) {
    return false;
  }

  let applied = false;
  for (const [field, messages] of Object.entries(errors)) {
    const control: AbstractControl | null = form.get(field);
    if (control && messages.length > 0) {
      control.setErrors({ ...control.errors, server: messages[0] });
      control.markAsTouched();
      applied = true;
    }
  }
  return applied;
}
