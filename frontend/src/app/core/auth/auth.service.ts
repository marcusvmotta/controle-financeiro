import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  Observable,
  catchError,
  finalize,
  firstValueFrom,
  map,
  of,
  shareReplay,
  tap,
  throwError,
} from 'rxjs';
import { AuthApi } from './auth.api';
import { AuthResponse, LoginRequest, RegisterRequest, User } from './auth.models';

/**
 * Estado da sessão (ADR-005).
 * O access token fica SÓ em memória (signal): some ao recarregar a página, e o tryRefresh() o recupera
 * usando o cookie HttpOnly do refresh token, que o JavaScript nunca vê.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApi);
  private readonly router = inject(Router);

  // Signals privados (só este service altera) + versões somente leitura públicas.
  private readonly _user = signal<User | null>(null);
  private readonly _accessToken = signal<string | null>(null);

  readonly user = this._user.asReadonly();
  readonly accessToken = this._accessToken.asReadonly();
  readonly isAuthenticated = computed(() => this._accessToken() !== null);

  /** Refresh em andamento, compartilhado por todos que pedirem um token novo ao mesmo tempo. */
  private refreshInFlight: Observable<string> | null = null;

  login(request: LoginRequest): Observable<User> {
    return this.api.login(request).pipe(map((response) => this.startSession(response)));
  }

  register(request: RegisterRequest): Observable<User> {
    return this.api.register(request).pipe(map((response) => this.startSession(response)));
  }

  /** Revoga o refresh token no servidor e limpa a sessão local, mesmo se o servidor não responder. */
  logout(): Observable<void> {
    return this.api.logout().pipe(
      catchError(() => of(undefined)),
      tap(() => {
        this.clearSession();
        void this.router.navigateByUrl('/login');
      }),
    );
  }

  /**
   * Chamado ao iniciar o app: se existir um cookie de refresh válido, recupera a sessão.
   * Nunca falha: sem sessão, o usuário simplesmente verá a tela de login.
   */
  async tryRestoreSession(): Promise<void> {
    try {
      await firstValueFrom(this.refreshAccessToken());
    } catch {
      // Sem sessão válida: segue deslogado.
    }
  }

  /**
   * Pede um novo access token usando o cookie do refresh token.
   * Se várias requisições receberem 401 ao mesmo tempo, todas recebem o MESMO refresh (shareReplay).
   * Isso é essencial: dois refreshes simultâneos com o mesmo cookie seriam vistos pelo backend como
   * "reuso de token" e derrubariam a sessão (ver M1a, detecção de reuso).
   */
  refreshAccessToken(): Observable<string> {
    this.refreshInFlight ??= this.api.refresh().pipe(
      map((response) => {
        this.startSession(response);
        return response.accessToken;
      }),
      catchError((error: unknown) => {
        this.clearSession();
        return throwError(() => error);
      }),
      finalize(() => (this.refreshInFlight = null)),
      shareReplay({ bufferSize: 1, refCount: false }),
    );

    return this.refreshInFlight;
  }

  /** A sessão expirou de vez (refresh falhou): limpa e manda para o login, voltando depois para onde estava. */
  handleSessionExpired(): void {
    this.clearSession();
    void this.router.navigate(['/login'], {
      queryParams: { returnUrl: this.router.url },
    });
  }

  private startSession(response: AuthResponse): User {
    this._accessToken.set(response.accessToken);
    this._user.set(response.user);
    return response.user;
  }

  private clearSession(): void {
    this._accessToken.set(null);
    this._user.set(null);
  }
}
