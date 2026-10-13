import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found-page',
  imports: [MatButtonModule, RouterLink],
  template: `
    <main class="auth-page">
      <div class="not-found">
        <h1>Página não encontrada</h1>
        <p>O endereço que você abriu não existe.</p>
        <a matButton="filled" routerLink="/">Voltar ao início</a>
      </div>
    </main>
  `,
  styles: `
    .not-found {
      text-align: center;
    }
  `,
})
export class NotFoundPage {}
