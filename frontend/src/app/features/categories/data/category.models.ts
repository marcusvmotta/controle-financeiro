// Espelham os DTOs da API (backend: Application/Features/Categories/CategoryContracts.cs).

export type CategoryType = 'Income' | 'Expense';

export interface Category {
  id: string;
  name: string;
  type: CategoryType;
  color: string;
  icon: string;
  isArchived: boolean;
  transactionCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  type: CategoryType;
  color: string;
  icon: string;
}

/** Sem "type": o tipo não muda depois de criada (regra do backend). */
export interface UpdateCategoryRequest {
  name: string;
  color: string;
  icon: string;
}

export const CATEGORY_TYPE_LABELS: Record<CategoryType, string> = {
  Expense: 'Despesa',
  Income: 'Receita',
};
