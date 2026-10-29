import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { COLOR_PALETTE } from './appearance';

/**
 * Grade de cores para escolher. Componente "de apresentação": recebe o valor atual (input)
 * e avisa quando o usuário escolhe outro (output). Quem guarda o valor é o formulário do pai.
 */
@Component({
  selector: 'app-color-picker',
  imports: [MatIconModule],
  template: `
    <div class="grid" role="radiogroup" aria-label="Cor">
      @for (color of colors; track color) {
        <button
          type="button"
          class="swatch"
          role="radio"
          [style.background]="color"
          [attr.aria-checked]="color === value()"
          [attr.aria-label]="'Cor ' + color"
          (click)="selected.emit(color)"
        >
          @if (color === value()) {
            <mat-icon>check</mat-icon>
          }
        </button>
      }
    </div>
  `,
  styles: `
    .grid {
      display: grid;
      grid-template-columns: repeat(6, 36px);
      gap: 8px;
    }
    .swatch {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      border: 2px solid transparent;
      color: white;
      cursor: pointer;
      display: grid;
      place-items: center;
      padding: 0;
    }
    .swatch[aria-checked='true'] {
      border-color: var(--mat-sys-on-surface);
    }
  `,
})
export class ColorPicker {
  readonly value = input.required<string>();
  readonly selected = output<string>();
  protected readonly colors = COLOR_PALETTE;
}
