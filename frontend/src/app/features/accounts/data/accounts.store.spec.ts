import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Account } from './account.models';
import { AccountsStore } from './accounts.store';

const nubank: Account = {
  id: '1',
  name: 'Nubank',
  type: 'Checking',
  initialBalance: 100,
  currentBalance: 100,
  color: '#8A05BE',
  icon: 'account_balance',
  isArchived: false,
};

describe('AccountsStore', () => {
  let store: AccountsStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AccountsStore, provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(AccountsStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('carrega as contas e o saldo total', () => {
    store.load();
    expect(store.status()).toBe('loading');

    http.expectOne('/api/accounts?includeArchived=false').flush({ items: [nubank], totalBalance: 100 });

    expect(store.status()).toBe('loaded');
    expect(store.accounts()).toEqual([nubank]);
    expect(store.totalBalance()).toBe(100);
    expect(store.isEmpty()).toBe(false);
  });

  it('isEmpty fica true só depois de carregar uma lista vazia', () => {
    store.load();
    expect(store.isEmpty()).toBe(false); // ainda carregando: não mostra "nenhuma conta"

    http.expectOne('/api/accounts?includeArchived=false').flush({ items: [], totalBalance: 0 });

    expect(store.isEmpty()).toBe(true);
  });

  it('recarrega a lista depois de criar (o total vem calculado do servidor)', () => {
    store.create({ name: 'Nubank', type: 'Checking', initialBalance: 100, color: '#8A05BE', icon: 'wallet' }).subscribe();
    http.expectOne({ method: 'POST', url: '/api/accounts' }).flush(nubank);

    http.expectOne('/api/accounts?includeArchived=false').flush({ items: [nubank], totalBalance: 100 });
    expect(store.accounts()).toHaveLength(1);
  });

  it('mostrar arquivadas refaz a busca com includeArchived=true', () => {
    store.toggleArchived(true);

    http.expectOne('/api/accounts?includeArchived=true').flush({ items: [], totalBalance: 0 });
    expect(store.showArchived()).toBe(true);
  });

  it('erro ao carregar vira estado de erro com mensagem', () => {
    store.load();
    http.expectOne('/api/accounts?includeArchived=false').flush(null, { status: 0, statusText: 'Unknown' });

    expect(store.status()).toBe('error');
    expect(store.error()).toContain('servidor');
  });
});
