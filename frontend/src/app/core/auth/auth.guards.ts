import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Guards decidem se uma rota pode ser aberta. Devolvem true (pode) ou uma UrlTree (redireciona).
 * Importante: guard é conveniência de navegação, não segurança. Quem protege os dados é a API (401).
 */

/** Rotas que exigem login. Sem sessão, vai para /login guardando para onde o usuário queria ir. */
export const authGuard: CanActivateFn = (_route, state) => {
  if (inject(AuthService).isAuthenticated()) {
    return true;
  }

  return inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/** Rotas só para visitantes (login, cadastro). Quem já está logado vai para o dashboard. */
export const guestGuard: CanActivateFn = () =>
  inject(AuthService).isAuthenticated() ? inject(Router).createUrlTree(['/dashboard']) : true;
