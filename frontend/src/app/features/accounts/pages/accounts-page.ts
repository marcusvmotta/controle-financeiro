import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { filter, switchMap } from 'rxjs';
import { errorMessage } from '../../../core/http/problem-details';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/components/confirm-dialog';
import { EmptyState } from '../../../shared/components/empty-state';
import { AccountDialog, AccountDialogData } from '../components/account-dialog';
import { ACCOUNT_TYPE_LABELS, Account } from '../data/account.models';
import { AccountsStore } from '../data/accounts.store';

@Component({
  selector: 'app-accounts-page',
  imports: [
    CurrencyPipe,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    EmptyState,
  ],
  providers: [AccountsStore],
  templateUrl: './accounts-page.html',
  styleUrl: './accounts-page.scss',
})
export class AccountsPage implements OnInit {
  protected readonly store = inject(AccountsStore);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly typeLabels = ACCOUNT_TYPE_LABELS;

  ngOnInit(): void {
    this.store.load();
  }

  protected openCreate(): void {
    this.dialog.open<AccountDialog, AccountDialogData>(AccountDialog, {
      width: '480px',
      data: { save: (request) => this.store.create(request) },
    });
  }

  protected openEdit(account: Account): void {
    this.dialog.open<AccountDialog, AccountDialogData>(AccountDialog, {
      width: '480px',
      data: { account, save: (request) => this.store.update(account.id, request) },
    });
  }

  protected setArchived(account: Account, archived: boolean): void {
    this.store.setArchived(account.id, archived).subscribe({
      next: () => this.snackBar.open(archived ? 'Conta arquivada.' : 'Conta reativada.', 'OK', { duration: 3000 }),
      error: (error: unknown) => this.snackBar.open(errorMessage(error), 'Fechar', { duration: 5000 }),
    });
  }

  protected confirmDelete(account: Account): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: {
          title: 'Excluir conta?',
          message: `A conta "${account.name}" será excluída definitivamente.`,
          confirmLabel: 'Excluir',
        },
      })
      .afterClosed()
      .pipe(
        filter((confirmed) => confirmed === true),
        switchMap(() => this.store.delete(account.id)),
      )
      .subscribe({
        next: () => this.snackBar.open('Conta excluída.', 'OK', { duration: 3000 }),
        error: (error: unknown) => this.handleDeleteError(account, error),
      });
  }

  /** 409 = a conta tem transações (a partir do M3). Em vez de só mostrar o erro, oferece arquivar. */
  private handleDeleteError(account: Account, error: unknown): void {
    if (error instanceof HttpErrorResponse && error.status === 409) {
      this.snackBar
        .open(`${errorMessage(error)} Deseja arquivar em vez disso?`, 'Arquivar', { duration: 8000 })
        .onAction()
        .subscribe(() => this.setArchived(account, true));
      return;
    }
    this.snackBar.open(errorMessage(error), 'Fechar', { duration: 5000 });
  }
}
