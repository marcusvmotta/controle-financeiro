import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Mensagem para listas vazias (TRD 05-frontend: toda tela trata os estados carregando, vazio e erro).
 * O conteúdo entre as tags (ex.: um botão "Criar") aparece embaixo, via <ng-content>.
 */
@Component({
  selector: 'app-empty-state',
  imports: [MatIconModule],
  template: `
    <div class="empty">
      <mat-icon class="icon">{{ icon() }}</mat-icon>
      <h2>{{ title() }}</h2>
      <p>{{ message() }}</p>
      <ng-content />
    </div>
  `,
  styles: `
    .empty {
      text-align: center;
      padding: 3rem 1rem;
      color: var(--mat-sys-on-surface-variant);
    }
    .icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }
    h2 {
      font: var(--mat-sys-title-large);
      color: var(--mat-sys-on-surface);
      margin-bottom: 0.25rem;
    }
  `,
})
export class EmptyState {
  readonly icon = input('inbox');
  readonly title = input.required<string>();
  readonly message = input('');
}
