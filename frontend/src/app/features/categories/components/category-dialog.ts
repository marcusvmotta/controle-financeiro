import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs';
import { applyServerValidationErrors, errorMessage } from '../../../core/http/problem-details';
import { CATEGORY_ICONS, COLOR_PALETTE } from '../../../shared/appearance/appearance';
import { ColorPicker } from '../../../shared/appearance/color-picker';
import { IconPicker } from '../../../shared/appearance/icon-picker';
import { CATEGORY_TYPE_LABELS, Category, CategoryType, CreateCategoryRequest } from '../data/category.models';

export interface CategoryDialogData {
  category?: Category;
  /** Tipo pré-selecionado ao criar (a aba que estava aberta). */
  defaultType: CategoryType;
  /** Recebe o pedido completo; na edição, quem abre ignora o "type". */
  save: (request: CreateCategoryRequest) => Observable<unknown>;
}

@Component({
  selector: 'app-category-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ColorPicker,
    IconPicker,
  ],
  templateUrl: './category-dialog.html',
  styleUrl: '../../../shared/appearance/appearance-dialog.scss',
})
export class CategoryDialog {
  protected readonly data = inject<CategoryDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CategoryDialog>);

  protected readonly isEdit = this.data.category !== undefined;
  protected readonly typeLabels = CATEGORY_TYPE_LABELS;
  protected readonly icons = CATEGORY_ICONS;
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    name: [this.data.category?.name ?? '', [Validators.required, Validators.maxLength(40)]],
    type: [this.data.category?.type ?? this.data.defaultType],
    color: [this.data.category?.color ?? COLOR_PALETTE[0]],
    icon: [this.data.category?.icon ?? CATEGORY_ICONS[0]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set(null);
    this.data.save({ ...value, name: value.name.trim() }).subscribe({
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
