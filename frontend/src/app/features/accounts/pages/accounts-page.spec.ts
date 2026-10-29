import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AccountsPage } from './accounts-page';

describe('AccountsPage', () => {
  let fixture: ComponentFixture<AccountsPage>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(AccountsPage);
    fixture.detectChanges(); // ngOnInit → load()
  });

  afterEach(() => http.verify());

  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  it('mostra as contas com saldo em reais e o total', async () => {
    http.expectOne('/api/accounts?includeArchived=false').flush({
      items: [
        { id: '1', name: 'Nubank', type: 'Checking', initialBalance: 1234.5, currentBalance: 1234.5, color: '#8A05BE', icon: 'account_balance', isArchived: false },
        { id: '2', name: 'Cartão', type: 'CreditCard', initialBalance: -200, currentBalance: -200, color: '#000000', icon: 'credit_card', isArchived: false },
      ],
      totalBalance: 1034.5,
    });
    await fixture.whenStable();

    expect(text()).toContain('Nubank');
    expect(text()).toContain('Conta corrente');
    expect(text()).toContain('Cartão de crédito');
    // Formatação brasileira (LOCALE_ID pt-BR vem do app.config; no teste, o pipe recebe o locale padrão).
    expect(text()).toMatch(/1[.,]?234[.,]50/);
    expect(text()).toContain('Saldo total');
  });

  it('lista vazia mostra o convite para cadastrar a primeira conta', async () => {
    http.expectOne('/api/accounts?includeArchived=false').flush({ items: [], totalBalance: 0 });
    await fixture.whenStable();

    expect(text()).toContain('Nenhuma conta ainda');
    expect(text()).toContain('Cadastrar primeira conta');
    expect(text()).not.toContain('Saldo total');
  });

  it('erro de carregamento mostra a mensagem e o botão de tentar de novo', async () => {
    http.expectOne('/api/accounts?includeArchived=false').flush(null, { status: 0, statusText: 'Unknown' });
    await fixture.whenStable();

    expect(text()).toContain('Tentar de novo');
  });
});
