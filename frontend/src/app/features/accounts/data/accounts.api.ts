import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Account, AccountList, AccountRequest } from './account.models';

/** Chamadas HTTP de contas. Só HTTP, sem estado. O token é colocado pelo authInterceptor. */
@Injectable({ providedIn: 'root' })
export class AccountsApi {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/accounts';

  list(includeArchived: boolean): Observable<AccountList> {
    return this.http.get<AccountList>(this.url, { params: { includeArchived } });
  }

  create(request: AccountRequest): Observable<Account> {
    return this.http.post<Account>(this.url, request);
  }

  update(id: string, request: AccountRequest): Observable<Account> {
    return this.http.put<Account>(`${this.url}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  setArchived(id: string, archived: boolean): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/${archived ? 'archive' : 'unarchive'}`, null);
  }
}
