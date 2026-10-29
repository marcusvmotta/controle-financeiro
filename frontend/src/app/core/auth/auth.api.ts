import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthConfig, AuthResponse, LoginRequest, RegisterRequest, User } from './auth.models';

/**
 * Chamadas HTTP de autenticação. Só HTTP, sem estado (o estado fica no AuthService).
 * O cookie do refresh token e o header X-Requested-With são tratados pelo authInterceptor.
 */
@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', request);
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', request);
  }

  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/refresh', null);
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/auth/logout', null);
  }

  config(): Observable<AuthConfig> {
    return this.http.get<AuthConfig>('/api/auth/config');
  }

  me(): Observable<User> {
    return this.http.get<User>('/api/me');
  }
}
