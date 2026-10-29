import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { authResponse } from '../../../core/auth/testing';
import { RegisterPage } from './register-page';

describe('RegisterPage', () => {
  let fixture: ComponentFixture<RegisterPage>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fixture = TestBed.createComponent(RegisterPage);
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  const page = () => fixture.nativeElement as HTMLElement;

  function fill(values: { name: string; email: string; password: string; confirm: string }): void {
    const set = (name: string, value: string) => {
      const input = page().querySelector<HTMLInputElement>(`input[formControlName=${name}]`)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
      input.dispatchEvent(new Event('blur')); // "touched": libera a exibição dos erros
    };
    set('name', values.name);
    set('email', values.email);
    set('password', values.password);
    set('confirmPassword', values.confirm);
  }

  async function submit(): Promise<void> {
    page().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  it('exige letra e número na senha', async () => {
    fill({ name: 'Ana', email: 'ana@email.com', password: 'semnumeros', confirm: 'semnumeros' });
    await submit();

    http.expectNone('/api/auth/register');
    expect(page().textContent).toContain('pelo menos uma letra e um número');
  });

  it('avisa quando as senhas não coincidem', async () => {
    fill({ name: 'Ana', email: 'ana@email.com', password: 'senha1234', confirm: 'senha9999' });
    await submit();

    http.expectNone('/api/auth/register');
    expect(page().textContent).toContain('As senhas não coincidem.');
  });

  it('envia nome, e-mail e senha (sem a confirmação) quando tudo é válido', async () => {
    fill({ name: '  Ana Souza ', email: 'ana@email.com', password: 'senha1234', confirm: 'senha1234' });
    await submit();

    const request = http.expectOne('/api/auth/register');
    expect(request.request.body).toEqual({ name: 'Ana Souza', email: 'ana@email.com', password: 'senha1234' });
    request.flush(authResponse('t'), { status: 201, statusText: 'Created' });
  });

  it('mostra no campo o erro de validação devolvido pela API', async () => {
    fill({ name: 'Ana', email: 'ana@email.com', password: 'senha1234', confirm: 'senha1234' });
    await submit();

    http.expectOne('/api/auth/register').flush(
      { title: 'Um ou mais campos são inválidos', status: 400, errors: { email: ['Este e-mail é estranho.'] } },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    expect(page().textContent).toContain('Este e-mail é estranho.');
  });

  it('mostra o conflito de e-mail já cadastrado', async () => {
    fill({ name: 'Ana', email: 'ana@email.com', password: 'senha1234', confirm: 'senha1234' });
    await submit();

    http.expectOne('/api/auth/register').flush(
      { title: 'Conflito', status: 409, detail: 'Este e-mail já está cadastrado.' },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    expect(page().querySelector('[role=alert]')?.textContent).toContain('Este e-mail já está cadastrado.');
  });
});
