import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Layout das páginas logadas: barra de topo + área onde as páginas aparecem (router-outlet).
 * O menu lateral com Contas, Categorias e Transações entra no M2.
 */
@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterOutlet, MatButtonModule, MatIconModule, MatMenuModule, MatToolbarModule],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  protected readonly auth = inject(AuthService);

  protected logout(): void {
    this.auth.logout().subscribe();
  }
}
