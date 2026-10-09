import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface PingResponse {
  message: string;
  serverTime: string;
}

export interface HealthResponse {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  checks: Record<string, string>;
}

/**
 * Chamadas HTTP de diagnóstico da API. Arquivos "*.api.ts" só fazem HTTP, sem estado (TRD 05-frontend).
 * As URLs são relativas: em dev o proxy.conf.json repassa para a API; em produção, o Nginx.
 */
@Injectable({ providedIn: 'root' })
export class SystemStatusApi {
  private readonly http = inject(HttpClient);

  ping(): Observable<PingResponse> {
    return this.http.get<PingResponse>('/api/ping');
  }

  readiness(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>('/health/ready');
  }
}
