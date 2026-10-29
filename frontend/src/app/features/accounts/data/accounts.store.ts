import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { errorMessage } from '../../../core/http/problem-details';
import { Account, AccountRequest } from './account.models';
import { AccountsApi } from './accounts.api';

export type LoadStatus = 'loading' | 'loaded' | 'error';

/**
 * Estado da tela de contas (TRD 05-frontend: "store" simples com signals, sem NgRx).
 * Fornecida pela própria página (providers: [AccountsStore]): cada visita à tela começa com estado limpo.
 */
@Injectable()
export class AccountsStore {
  private readonly api = inject(AccountsApi);

  private readonly _accounts = signal<Account[]>([]);
  private readonly _totalBalance = signal(0);
  private readonly _status = signal<LoadStatus>('loading');
  private readonly _error = signal<string | null>(null);

  readonly accounts = this._accounts.asReadonly();
  readonly totalBalance = this._totalBalance.asReadonly();
  readonly status = this._status.asReadonly();
  readonly error = this._error.asReadonly();

  /** Mostrar também as contas arquivadas? Ligado ao "toggle" da tela. */
  readonly showArchived = signal(false);

  readonly isEmpty = computed(() => this._status() === 'loaded' && this._accounts().length === 0);

  load(): void {
    this._status.set('loading');
    this.api.list(this.showArchived()).subscribe({
      next: (list) => {
        this._accounts.set(list.items);
        this._totalBalance.set(list.totalBalance);
        this._status.set('loaded');
      },
      error: (error: unknown) => {
        this._error.set(errorMessage(error));
        this._status.set('error');
      },
    });
  }

  toggleArchived(show: boolean): void {
    this.showArchived.set(show);
    this.load();
  }

  // Depois de qualquer alteração, recarrega a lista: o saldo total vem calculado do servidor.

  create(request: AccountRequest): Observable<Account> {
    return this.api.create(request).pipe(tap(() => this.load()));
  }

  update(id: string, request: AccountRequest): Observable<Account> {
    return this.api.update(id, request).pipe(tap(() => this.load()));
  }

  delete(id: string): Observable<void> {
    return this.api.delete(id).pipe(tap(() => this.load()));
  }

  setArchived(id: string, archived: boolean): Observable<void> {
    return this.api.setArchived(id, archived).pipe(tap(() => this.load()));
  }
}
