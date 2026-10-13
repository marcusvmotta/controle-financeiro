import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';
import { SystemStatusApi } from '../../core/system-status/system-status.api';

type CheckState = 'loading' | 'ok' | 'error';

interface StatusItem {
  state: CheckState;
  detail: string;
}

/**
 * Dashboard provisório: saudação + status do sistema (o "hello world" do M0).
 * Os cards e gráficos reais entram no M4.
 */
@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe, MatButtonModule, MatCardModule, MatIconModule],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage implements OnInit {
  private readonly statusApi = inject(SystemStatusApi);
  protected readonly user = inject(AuthService).user;

  protected readonly api = signal<StatusItem>({ state: 'loading', detail: 'Verificando...' });
  protected readonly database = signal<StatusItem>({ state: 'loading', detail: 'Verificando...' });
  protected readonly serverTime = signal<string | null>(null);

  ngOnInit(): void {
    this.check();
  }

  protected check(): void {
    this.api.set({ state: 'loading', detail: 'Verificando...' });
    this.database.set({ state: 'loading', detail: 'Verificando...' });

    this.statusApi.ping().subscribe({
      next: (response) => {
        this.api.set({ state: 'ok', detail: `Respondeu "${response.message}"` });
        this.serverTime.set(response.serverTime);
      },
      error: () => this.api.set({ state: 'error', detail: 'Sem resposta da API' }),
    });

    this.statusApi.readiness().subscribe({
      next: (response) =>
        this.database.set({
          state: response.status === 'Healthy' ? 'ok' : 'error',
          detail: response.status === 'Healthy' ? 'Conectado ao PostgreSQL' : 'Banco indisponível',
        }),
      // /health/ready responde 503 quando o banco está fora: o HttpClient trata como erro.
      error: () => this.database.set({ state: 'error', detail: 'Banco indisponível' }),
    });
  }
}
