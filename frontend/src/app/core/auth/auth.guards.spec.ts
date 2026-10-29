import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { authGuard, guestGuard } from './auth.guards';
import { AuthService } from './auth.service';
import { authResponse } from './testing';

describe('guards de autenticação', () => {
  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/transactions?month=10' } as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
  });

  function login(): void {
    TestBed.inject(AuthService).login({ email: 'a@b.com', password: 'senha1234' }).subscribe();
    TestBed.inject(HttpTestingController).expectOne('/api/auth/login').flush(authResponse('t'));
  }

  // Guards funcionais usam inject(), então precisam rodar dentro de um "contexto de injeção".
  const runAuthGuard = () => TestBed.runInInjectionContext(() => authGuard(route, state));
  const runGuestGuard = () => TestBed.runInInjectionContext(() => guestGuard(route, state));

  it('authGuard: sem sessão, redireciona para /login guardando o destino', () => {
    const result = runAuthGuard() as UrlTree;

    expect(TestBed.inject(Router).serializeUrl(result)).toBe(
      '/login?returnUrl=%2Ftransactions%3Fmonth%3D10',
    );
  });

  it('authGuard: com sessão, libera', () => {
    login();
    expect(runAuthGuard()).toBe(true);
  });

  it('guestGuard: sem sessão, libera o login', () => {
    expect(runGuestGuard()).toBe(true);
  });

  it('guestGuard: com sessão, manda para o dashboard', () => {
    login();
    const result = runGuestGuard() as UrlTree;

    expect(TestBed.inject(Router).serializeUrl(result)).toBe('/dashboard');
  });
});
