import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { errorMessage } from './problem-details';

/**
 * Avisa o usuário sobre erros que nenhuma tela trata: servidor fora do ar (status 0) e erros 5xx.
 * Erros 4xx (validação, conflito, não encontrado) são tratados por cada tela, perto do formulário.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const snackBar = inject(MatSnackBar);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && (error.status === 0 || error.status >= 500)) {
        snackBar.open(errorMessage(error), 'Fechar', { duration: 6000 });
      }
      return throwError(() => error);
    }),
  );
};
