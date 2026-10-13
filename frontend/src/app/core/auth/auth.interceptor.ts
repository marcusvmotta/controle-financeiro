import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Interceptor: uma função que "envolve" toda requisição HTTP feita pelo HttpClient.
 *
 * 1. Rotas /api/auth/*: envia o cookie e o header anti-CSRF exigido pelo backend.
 * 2. Demais rotas /api/*: anexa "Authorization: Bearer <token>".
 * 3. Se a API responder 401 (token expirado), renova a sessão UMA vez e repete a requisição.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) {
    return next(request);
  }

  if (request.url.startsWith('/api/auth/')) {
    return next(
      request.clone({
        withCredentials: true,
        setHeaders: { 'X-Requested-With': 'XMLHttpRequest' },
      }),
    );
  }

  const auth = inject(AuthService);

  return next(withToken(request, auth.accessToken())).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      return auth.refreshAccessToken().pipe(
        // Se nem o refresh funcionar, a sessão acabou: vai para o login e repassa o erro ORIGINAL.
        catchError(() => {
          auth.handleSessionExpired();
          return throwError(() => error);
        }),
        // Refresh deu certo: repete a requisição original com o token novo.
        switchMap((newToken) => next(withToken(request, newToken))),
      );
    }),
  );
};

function withToken(request: HttpRequest<unknown>, token: string | null): HttpRequest<unknown> {
  return token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;
}
