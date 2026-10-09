import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { SystemStatusApi } from './core/system-status/system-status.api';

type CheckState = 'loading' | 'ok' | 'error';

interface StatusItem {
  state: CheckState;
  detail: string;
}

/**
 * Página temporária do M0: mostra se o frontend fala com a API e se a API fala com o banco.
 * Será substituída pelo layout real (login, dashboard) a partir do M1.
 */
@Component({
  selector: 'app-root',
  imports: [DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly statusApi = inject(SystemStatusApi);

  // signal(): valor reativo. Quando muda (com .set), o Angular atualiza só o que depende dele na tela.
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
