import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { errorMessage } from '../../../core/http/problem-details';
import { LoadStatus } from '../../accounts/data/accounts.store';
import { CategoriesApi } from './categories.api';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from './category.models';

/**
 * Estado da tela de categorias. Carrega TODAS as categorias de uma vez (são poucas)
 * e separa por tipo com computed(): trocar de aba não faz nova chamada à API.
 */
@Injectable()
export class CategoriesStore {
  private readonly api = inject(CategoriesApi);

  private readonly _categories = signal<Category[]>([]);
  private readonly _status = signal<LoadStatus>('loading');
  private readonly _error = signal<string | null>(null);

  readonly status = this._status.asReadonly();
  readonly error = this._error.asReadonly();
  readonly showArchived = signal(false);

  readonly expenses = computed(() => this._categories().filter((c) => c.type === 'Expense'));
  readonly incomes = computed(() => this._categories().filter((c) => c.type === 'Income'));

  load(): void {
    this._status.set('loading');
    this.api.list(this.showArchived()).subscribe({
      next: (categories) => {
        this._categories.set(categories);
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

  create(request: CreateCategoryRequest): Observable<Category> {
    return this.api.create(request).pipe(tap(() => this.load()));
  }

  update(id: string, request: UpdateCategoryRequest): Observable<Category> {
    return this.api.update(id, request).pipe(tap(() => this.load()));
  }

  delete(id: string): Observable<void> {
    return this.api.delete(id).pipe(tap(() => this.load()));
  }

  setArchived(id: string, archived: boolean): Observable<void> {
    return this.api.setArchived(id, archived).pipe(tap(() => this.load()));
  }
}
