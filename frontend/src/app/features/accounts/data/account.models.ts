// Espelham os DTOs da API (backend: Application/Features/Accounts/AccountContracts.cs).

export type AccountType = 'Checking' | 'Savings' | 'Cash' | 'CreditCard' | 'Other';

export interface Account {
  id: string;
  name: string;
  type: AccountType;
  initialBalance: number;
  currentBalance: number;
  color: string;
  icon: string;
  isArchived: boolean;
}

export interface AccountList {
  items: Account[];
  totalBalance: number;
}

export interface AccountRequest {
  name: string;
  type: AccountType;
  initialBalance: number;
  color: string;
  icon: string;
}

/** Nomes em português para exibir na tela (o valor técnico continua em inglês). */
export const ACCOUNT_TYPE_LABELS: Record<AccountType, string> = {
  Checking: 'Conta corrente',
  Savings: 'Poupança',
  Cash: 'Dinheiro',
  CreditCard: 'Cartão de crédito',
  Other: 'Outro',
};
