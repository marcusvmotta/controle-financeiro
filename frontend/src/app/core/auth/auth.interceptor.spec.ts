import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { authResponse } from './testing';

describe('authInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let auth: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
  });

  afterEach(() => http.verify());

  function loginWithToken(token: string): void {
    auth.login({ email: 'ana@email.com', password: 'senha1234' }).subscribe();
    http.expectOne('/api/auth/login').flush(authResponse(token));
  }

  it('anexa o Bearer token nas chamadas da API', () => {
    loginWithToken('abc');

    client.get('/api/me').subscribe();

    expect(http.expectOne('/api/me').request.headers.get('Authorization')).toBe('Bearer abc');
  });

  it('não mexe em requisições fora de /api', () => {
    loginWithToken('abc');

    client.get('/assets/config.json').subscribe();

    expect(http.expectOne('/assets/config.json').request.headers.has('Authorization')).toBe(false);
  });

  it('rotas /api/auth/* vão com cookie e header anti-CSRF, sem Bearer', () => {
    loginWithToken('abc');

    auth.refreshAccessToken().subscribe();
    const refresh = http.expectOne('/api/auth/refresh');

    expect(refresh.request.withCredentials).toBe(true);
    expect(refresh.request.headers.get('X-Requested-With')).toBe('XMLHttpRequest');
    expect(refresh.request.headers.has('Authorization')).toBe(false);
    refresh.flush(authResponse('def'));
  });

  // Teste obrigatório nº 10 do TRD (08-testes).
  it('duas requisições com 401 simultâneo disparam UM refresh e ambas são repetidas com o token novo', () => {
    loginWithToken('velho');
    const results: unknown[] = [];

    client.get('/api/accounts').subscribe((r) => results.push(r));
    client.get('/api/categories').subscribe((r) => results.push(r));

    // O token expirou: a API responde 401 para as duas.
    http.expectOne('/api/accounts').flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/categories').flush(null, { status: 401, statusText: 'Unauthorized' });

    // Um único refresh (expectOne falharia se houvesse dois).
    http.expectOne('/api/auth/refresh').flush(authResponse('novo'));

    // As duas requisições são repetidas, agora com o token novo.
    const retryAccounts = http.expectOne('/api/accounts');
    const retryCategories = http.expectOne('/api/categories');
    expect(retryAccounts.request.headers.get('Authorization')).toBe('Bearer novo');
    expect(retryCategories.request.headers.get('Authorization')).toBe('Bearer novo');
    retryAccounts.flush(['conta']);
    retryCategories.flush(['categoria']);

    expect(results).toEqual([['conta'], ['categoria']]);
  });

  it('se o refresh falhar, encerra a sessão, vai para o login e repassa o erro original', () => {
    loginWithToken('velho');
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    let status: number | undefined;

    client.get('/api/accounts').subscribe({ error: (e: { status: number }) => (status = e.status) });
    http.expectOne('/api/accounts').flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(status).toBe(401);
    expect(auth.isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login'], expect.anything());
  });

  it('erros que não são 401 passam direto, sem refresh', () => {
    loginWithToken('abc');
    let status: number | undefined;

    client.get('/api/accounts').subscribe({ error: (e: { status: number }) => (status = e.status) });
    http.expectOne('/api/accounts').flush(null, { status: 404, statusText: 'Not Found' });

    expect(status).toBe(404);
    http.expectNone('/api/auth/refresh');
  });
});
