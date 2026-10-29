import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from './category.models';

/** Chamadas HTTP de categorias. Só HTTP, sem estado. */
@Injectable({ providedIn: 'root' })
export class CategoriesApi {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/categories';

  list(includeArchived: boolean): Observable<Category[]> {
    return this.http.get<Category[]>(this.url, { params: { includeArchived } });
  }

  create(request: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.url, request);
  }

  update(id: string, request: UpdateCategoryRequest): Observable<Category> {
    return this.http.put<Category>(`${this.url}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  setArchived(id: string, archived: boolean): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/${archived ? 'archive' : 'unarchive'}`, null);
  }
}
