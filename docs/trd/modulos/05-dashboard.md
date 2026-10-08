# Módulo 5 — Dashboard (técnico)

[← TRD](../README.md) · **PRD:** [RF-05](../../prd/modulos/05-dashboard.md) · **Decisões:** [ADR-011](../adr/011-graficos-ng2-charts.md), [ADR-012](../adr/012-dinheiro-e-datas.md)

## Endpoints

Todos aceitam `from` e `to` (padrão: mês atual no fuso `America/Sao_Paulo`). Todas as agregações são feitas **no banco**, e todas **excluem transferências** (`type IN ('Income', 'Expense')`).

### `GET /api/dashboard/summary?from=&to=`
```json
{
  "from": "2026-10-01",
  "to": "2026-10-31",
  "totalIncome": 8500.00,
  "totalExpense": 6230.75,
  "periodBalance": 2269.25,
  "accountsBalance": 15120.90,
  "recentTransactions": [ /* 5 últimas, mesmo formato da listagem */ ]
}
```
`accountsBalance` é o saldo atual de todas as contas não arquivadas (independe do período), igual ao `totalBalance` de `/api/accounts`.

### `GET /api/dashboard/expenses-by-category?from=&to=`
```json
[
  { "categoryId": "0192...", "name": "Moradia", "color": "#3F51B5", "icon": "home", "total": 2500.00, "percentage": 40.12 },
  { "categoryId": "0192...", "name": "Alimentação", "color": "#4CAF50", "icon": "restaurant", "total": 1320.40, "percentage": 21.19 }
]
```
Ordenado por `total` decrescente. `percentage` calculado no backend; arredondado a 2 casas.

### `GET /api/dashboard/monthly-evolution?months=12`
```json
[
  { "month": "2025-11", "income": 8000.00, "expense": 6100.00 },
  { "month": "2025-12", "income": 9200.00, "expense": 7350.20 }
]
```
- `months` entre 1 e 24 (padrão 12), terminando no mês atual.
- Meses sem movimento aparecem com zero (preenchidos no service), para o gráfico não "pular" meses.

## Implementação

- Agregação com `GroupBy` do EF Core sobre `date`, traduzida para `date_trunc('month', date)` (conferir o SQL gerado; se o EF não traduzir bem, usar `FromSql` com SQL parametrizado e comentar o motivo).
- `TimeProvider` fornece "hoje" no fuso do usuário, para os testes fixarem a data.
- **Consistência com a listagem** (teste obrigatório 7): o dashboard e `GET /api/transactions` usam o **mesmo método** de filtro por período/usuário.

## Frontend

- Seletor de período no topo (mês com setas ◀ ▶ e opção "personalizado").
- Quatro cards de resumo (`mat-card`).
- Gráfico de rosca (despesas por categoria) com legenda; clicar numa fatia navega para `/transactions?categoryId=...&from=...&to=...` (P1 do PRD).
- Gráfico de barras agrupadas (receitas × despesas por mês).
- Lista das 5 últimas transações com link "Ver todas".
- Estado vazio para usuário novo: ilustração + botões "Cadastrar conta" e "Lançar transação".
- As três chamadas são feitas **em paralelo**; cada bloco mostra seu próprio carregamento.
