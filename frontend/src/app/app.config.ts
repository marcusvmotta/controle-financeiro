import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import localePt from '@angular/common/locales/pt';
import {
  ApplicationConfig,
  LOCALE_ID,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import { MatIconRegistry } from '@angular/material/icon';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { AuthService } from './core/auth/auth.service';
import { errorInterceptor } from './core/http/error.interceptor';

// Formatação brasileira: R$ 1.234,56 e datas dd/MM/yyyy (TRD 05-frontend, "Formatação").
registerLocaleData(localePt);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),

    // Interceptors rodam na ordem da lista: o primeiro "envolve" os seguintes.
    // errorInterceptor fica por fora para só ver o erro final (depois de o authInterceptor tentar o refresh).
    provideHttpClient(withFetch(), withInterceptors([errorInterceptor, authInterceptor])),

    { provide: LOCALE_ID, useValue: 'pt-BR' },
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline' } },

    // Roda ANTES da primeira rota: ícones do Material e recuperação da sessão pelo cookie.
    // Assim os guards já sabem se o usuário está logado quando decidem a primeira navegação.
    provideAppInitializer(() => {
      inject(MatIconRegistry).setDefaultFontSetClass('material-symbols-outlined');
      return inject(AuthService).tryRestoreSession();
    }),
  ],
};
