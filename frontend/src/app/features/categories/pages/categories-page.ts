import { NgTemplateOutlet } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { filter, switchMap } from 'rxjs';
import { errorMessage } from '../../../core/http/problem-details';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/components/confirm-dialog';
import { EmptyState } from '../../../shared/components/empty-state';
import { CategoryDialog, CategoryDialogData } from '../components/category-dialog';
import { CategoriesStore } from '../data/categories.store';
import { Category, CategoryType } from '../data/category.models';

@Component({
  selector: 'app-categories-page',
  imports: [
    NgTemplateOutlet,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatTabsModule,
    EmptyState,
  ],
  providers: [CategoriesStore],
  templateUrl: './categories-page.html',
  styleUrl: './categories-page.scss',
})
export class CategoriesPage implements OnInit {
  protected readonly store = inject(CategoriesStore);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  /** Aba aberta: 0 = Despesas, 1 = Receitas. Define o tipo pré-selecionado ao criar. */
  protected readonly selectedTab = signal(0);

  ngOnInit(): void {
    this.store.load();
  }

  protected openCreate(): void {
    const defaultType: CategoryType = this.selectedTab() === 0 ? 'Expense' : 'Income';
    this.dialog.open<CategoryDialog, CategoryDialogData>(CategoryDialog, {
      width: '480px',
      data: { defaultType, save: (request) => this.store.create(request) },
    });
  }

  protected openEdit(category: Category): void {
    this.dialog.open<CategoryDialog, CategoryDialogData>(CategoryDialog, {
      width: '480px',
      data: {
        category,
        defaultType: category.type,
        save: ({ name, color, icon }) => this.store.update(category.id, { name, color, icon }),
      },
    });
  }

  protected setArchived(category: Category, archived: boolean): void {
    this.store.setArchived(category.id, archived).subscribe({
      next: () =>
        this.snackBar.open(archived ? 'Categoria arquivada.' : 'Categoria reativada.', 'OK', { duration: 3000 }),
      error: (error: unknown) => this.snackBar.open(errorMessage(error), 'Fechar', { duration: 5000 }),
    });
  }

  protected confirmDelete(category: Category): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: {
          title: 'Excluir categoria?',
          message: `A categoria "${category.name}" será excluída definitivamente.`,
          confirmLabel: 'Excluir',
        },
      })
      .afterClosed()
      .pipe(
        filter((confirmed) => confirmed === true),
        switchMap(() => this.store.delete(category.id)),
      )
      .subscribe({
        next: () => this.snackBar.open('Categoria excluída.', 'OK', { duration: 3000 }),
        error: (error: unknown) => {
          // 409 = categoria em uso por transações (a partir do M3): oferece arquivar.
          if (error instanceof HttpErrorResponse && error.status === 409) {
            this.snackBar
              .open(`${errorMessage(error)} Deseja arquivar em vez disso?`, 'Arquivar', { duration: 8000 })
              .onAction()
              .subscribe(() => this.setArchived(category, true));
            return;
          }
          this.snackBar.open(errorMessage(error), 'Fechar', { duration: 5000 });
        },
      });
  }
}
