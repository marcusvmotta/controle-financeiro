import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { applyServerValidationErrors, errorMessage } from '../../../core/http/problem-details';
import { ACCOUNT_ICONS, COLOR_PALETTE } from '../../../shared/appearance/appearance';
import { ColorPicker } from '../../../shared/appearance/color-picker';
import { IconPicker } from '../../../shared/appearance/icon-picker';
import { formatMoneyForInput, moneyValidator, parseMoney } from '../../../shared/money/money';
import { ACCOUNT_TYPE_LABELS, Account, AccountRequest, AccountType } from '../data/account.models';

export interface AccountDialogData {
  /** Conta a editar; ausente = criar uma nova. */
  account?: Account;
  /** Quem abre o diálogo diz como salvar. O diálogo só fecha se salvar der certo. */
  save: (request: AccountRequest) => Observable<unknown>;
}

@Component({
  selector: 'app-account-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ColorPicker,
    IconPicker,
  ],
  templateUrl: './account-dialog.html',
  styleUrl: '../../../shared/appearance/appearance-dialog.scss',
})
export class AccountDialog {
  protected readonly data = inject<AccountDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<AccountDialog>);

  protected readonly isEdit = this.data.account !== undefined;
  protected readonly typeOptions = Object.entries(ACCOUNT_TYPE_LABELS) as [AccountType, string][];
  protected readonly icons = ACCOUNT_ICONS;
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    name: [this.data.account?.name ?? '', [Validators.required, Validators.maxLength(60)]],
    type: [this.data.account?.type ?? ('Checking' as AccountType), [Validators.required]],
    // Texto, para aceitar "1.234,56"; vira número só na hora de enviar.
    initialBalance: [formatMoneyForInput(this.data.account?.initialBalance ?? 0), [Validators.required, moneyValidator]],
    color: [this.data.account?.color ?? COLOR_PALETTE[0]],
    icon: [this.data.account?.icon ?? ACCOUNT_ICONS[0]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: AccountRequest = {
      name: value.name.trim(),
      type: value.type,
      initialBalance: parseMoney(value.initialBalance) ?? 0,
      color: value.color,
      icon: value.icon,
    };

    this.saving.set(true);
    this.errorMessage.set(null);
    this.data.save(request).subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: unknown) => {
        this.saving.set(false);
        if (!applyServerValidationErrors(this.form, error)) {
          this.errorMessage.set(errorMessage(error));
        }
      },
    });
  }
}
