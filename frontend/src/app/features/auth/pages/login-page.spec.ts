import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { authResponse } from '../../../core/auth/testing';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let http: HttpTestingController;
  let navigate: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fixture = TestBed.createComponent(LoginPage);
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  // Interage pelo HTML, como um usuário faria (e não mexendo nas variáveis internas do componente).
  const page = () => fixture.nativeElement as HTMLElement;

  function type(selector: string, value: string): void {
    const input = page().querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<void> {
    page().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  it('não chama a API com o formulário inválido e mostra os erros', async () => {
    await submit();

    http.expectNone('/api/auth/login');
    expect(page().textContent).toContain('Informe o e-mail.');
    expect(page().textContent).toContain('Informe a senha.');
  });

  it('login com sucesso vai para o dashboard', async () => {
    type('input[formControlName=email]', 'ana@email.com');
    type('input[formControlName=password]', 'senha1234');
    await submit();

    const request = http.expectOne('/api/auth/login');
    expect(request.request.body).toEqual({ email: 'ana@email.com', password: 'senha1234' });
    request.flush(authResponse('t'));

    expect(navigate).toHaveBeenCalledWith('/dashboard');
  });

  it('volta para o returnUrl depois do login', async () => {
    fixture.componentRef.setInput('returnUrl', '/transactions');
    type('input[formControlName=email]', 'ana@email.com');
    type('input[formControlName=password]', 'senha1234');
    await submit();

    http.expectOne('/api/auth/login').flush(authResponse('t'));

    expect(navigate).toHaveBeenCalledWith('/transactions');
  });

  it('ignora returnUrl que aponta para fora do site', async () => {
    fixture.componentRef.setInput('returnUrl', '//site-malicioso.com');
    type('input[formControlName=email]', 'ana@email.com');
    type('input[formControlName=password]', 'senha1234');
    await submit();

    http.expectOne('/api/auth/login').flush(authResponse('t'));

    expect(navigate).toHaveBeenCalledWith('/dashboard');
  });

  it('mostra a mensagem da API quando as credenciais são inválidas', async () => {
    type('input[formControlName=email]', 'ana@email.com');
    type('input[formControlName=password]', 'errada123');
    await submit();

    http
      .expectOne('/api/auth/login')
      .flush(
        { title: 'Não autenticado', status: 401, detail: 'E-mail ou senha inválidos.' },
        { status: 401, statusText: 'Unauthorized' },
      );
    await fixture.whenStable();

    expect(page().querySelector('[role=alert]')?.textContent).toContain('E-mail ou senha inválidos.');
    expect(navigate).not.toHaveBeenCalled();
  });
});
