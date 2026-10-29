import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { authResponse, testUser } from './testing';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('começa sem sessão', () => {
    expect(auth.isAuthenticated()).toBe(false);
    expect(auth.user()).toBeNull();
  });

  it('login guarda o usuário e o access token em memória', () => {
    auth.login({ email: 'ana@email.com', password: 'senha1234' }).subscribe();
    http.expectOne('/api/auth/login').flush(authResponse('token-1'));

    expect(auth.isAuthenticated()).toBe(true);
    expect(auth.accessToken()).toBe('token-1');
    expect(auth.user()).toEqual(testUser);
  });

  it('refreshAccessToken chamado várias vezes ao mesmo tempo faz UMA requisição só', () => {
    const tokens: string[] = [];

    auth.refreshAccessToken().subscribe((t) => tokens.push(t));
    auth.refreshAccessToken().subscribe((t) => tokens.push(t));
    auth.refreshAccessToken().subscribe((t) => tokens.push(t));

    // expectOne falha se houver mais de uma requisição para a URL.
    http.expectOne('/api/auth/refresh').flush(authResponse('novo'));

    expect(tokens).toEqual(['novo', 'novo', 'novo']);
  });

  it('depois de um refresh terminar, o próximo faz uma requisição nova', () => {
    auth.refreshAccessToken().subscribe();
    http.expectOne('/api/auth/refresh').flush(authResponse('primeiro'));

    auth.refreshAccessToken().subscribe();
    http.expectOne('/api/auth/refresh').flush(authResponse('segundo'));

    expect(auth.accessToken()).toBe('segundo');
  });

  it('refresh com falha limpa a sessão', () => {
    auth.login({ email: 'ana@email.com', password: 'senha1234' }).subscribe();
    http.expectOne('/api/auth/login').flush(authResponse('token-1'));

    auth.refreshAccessToken().subscribe({ error: () => undefined });
    http.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(auth.isAuthenticated()).toBe(false);
  });

  it('tryRestoreSession nunca lança erro, mesmo sem sessão', async () => {
    const restoring = auth.tryRestoreSession();
    http.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(restoring).resolves.toBeUndefined();
    expect(auth.isAuthenticated()).toBe(false);
  });

  it('logout limpa a sessão e vai para /login, mesmo se o servidor falhar', () => {
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    auth.login({ email: 'ana@email.com', password: 'senha1234' }).subscribe();
    http.expectOne('/api/auth/login').flush(authResponse('token-1'));

    auth.logout().subscribe();
    http.expectOne('/api/auth/logout').flush(null, { status: 500, statusText: 'Server Error' });

    expect(auth.isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith('/login');
  });
});
