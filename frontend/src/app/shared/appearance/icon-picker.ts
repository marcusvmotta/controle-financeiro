import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/** Grade de ícones do Material Symbols, no mesmo esquema do ColorPicker (input + output). */
@Component({
  selector: 'app-icon-picker',
  imports: [MatIconModule],
  template: `
    <div class="grid" role="radiogroup" aria-label="Ícone">
      @for (icon of icons(); track icon) {
        <button
          type="button"
          class="option"
          role="radio"
          [attr.aria-checked]="icon === value()"
          [attr.aria-label]="'Ícone ' + icon"
          [style.color]="icon === value() ? color() : null"
          (click)="selected.emit(icon)"
        >
          <mat-icon>{{ icon }}</mat-icon>
        </button>
      }
    </div>
  `,
  styles: `
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, 40px);
      gap: 4px;
    }
    .option {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      border: 1px solid transparent;
      background: none;
      color: var(--mat-sys-on-surface-variant);
      cursor: pointer;
      display: grid;
      place-items: center;
    }
    .option[aria-checked='true'] {
      border-color: currentColor;
      background: var(--mat-sys-surface-container-high);
    }
  `,
})
export class IconPicker {
  readonly icons = input.required<readonly string[]>();
  readonly value = input.required<string>();
  /** Cor usada para destacar o ícone escolhido (a cor escolhida para a conta/categoria). */
  readonly color = input<string>('currentColor');
  readonly selected = output<string>();
}
