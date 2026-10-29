import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/** Componente raiz: só um "buraco" onde o roteador encaixa a página atual. */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class App {}
