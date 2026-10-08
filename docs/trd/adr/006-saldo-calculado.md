# ADR-006 — Saldo da conta calculado na consulta

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T4

## Contexto

O saldo de uma conta é `saldo inicial + entradas − saídas`. Ele pode ser calculado a cada consulta ou guardado numa coluna atualizada a cada transação criada, editada ou excluída.

## Decisão

**Calcular na consulta**, com uma agregação no banco:

```sql
initial_balance
  + SUM(amount) FILTER (WHERE type IN ('Income', 'TransferIn'))
  - SUM(amount) FILTER (WHERE type IN ('Expense', 'TransferOut'))
```

O índice em `transactions(account_id)` mantém a consulta rápida.

## Alternativas consideradas

- **Coluna `current_balance` na conta**: leitura mais rápida, mas toda escrita de transação precisa atualizar o saldo na mesma transação de banco, com cuidado de concorrência. Um bug deixa o saldo errado para sempre.

## Consequências

- ✅ O saldo é sempre correto por construção, sem dados duplicados.
- ✅ Editar o saldo inicial (questão Q1 do PRD) não exige recalcular nada.
- ⚠️ Custo cresce com o número de transações. Com o volume de uma pessoa (milhares por ano), é desprezível. Se um dia virar problema, a saída é uma tabela de saldos mensais consolidados, sem mudar a API.
