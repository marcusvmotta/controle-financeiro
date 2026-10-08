# Módulo 4 — Transações

[← Índice](../README.md) · **Fase:** MVP · **Requisito:** RF-04 · **Prioridade:** P0

## Objetivo

Ser o coração do sistema: registrar cada entrada, saída e movimentação entre contas.

## Histórias de usuário

- Como **usuário**, quero lançar uma despesa em poucos segundos (valor, descrição, categoria, conta, data).
- Como **usuário**, quero registrar uma receita, como o meu salário.
- Como **usuário**, quero transferir dinheiro entre minhas contas sem que isso conte como gasto.
- Como **usuário**, quero filtrar e buscar transações por período, conta, categoria e descrição.
- Como **usuário**, quero corrigir ou excluir um lançamento errado.

## Requisitos

- Tipos: **Receita**, **Despesa**, **Transferência** (entre contas do próprio usuário).
- Campos: descrição, valor, data, conta, categoria (exceto transferência), observação opcional.
- Listagem com **paginação**, **filtros** (período, conta, categoria, tipo) e **busca** por descrição.
- Editar e excluir.

## Critérios de aceite

- [ ] Valor deve ser > 0 (o tipo define se entra ou sai).
- [ ] Transferência gera dois lançamentos vinculados; editar ou excluir um afeta ambos.
- [ ] Transferência não entra nos totais de receita e despesa do dashboard.
- [ ] Filtro padrão: mês atual.
- [ ] Lista responde em < 500 ms com 10 mil transações.
- [ ] Não é possível lançar em conta arquivada ou de outro usuário.

## Fora de escopo

- Anexos (foto de recibo).
- Parcelamento.
- Divisão de uma transação em várias categorias (*split*).

## Insumos para o TRD

- **Entidade:** `Transaction (Id, UserId, AccountId, CategoryId?, Type[Income|Expense|Transfer], Amount, Date, Description, Notes?, TransferPairId?, CreatedAt, UpdatedAt)`.
- **Endpoints preliminares:**
  - `GET /api/transactions?from=&to=&accountId=&categoryId=&type=&search=&page=&pageSize=`
  - `POST /api/transactions`, `POST /api/transactions/transfer`
  - `PUT /api/transactions/{id}`, `DELETE /api/transactions/{id}`
- **Decidir no TRD:**
  - Modelagem da transferência: dois registros com `TransferPairId` ou uma entidade `Transfer` separada?
  - Índices: `(UserId, Date)`, `(UserId, CategoryId)`, `(AccountId)`.
  - Busca por descrição: `ILIKE` simples ou full-text search do Postgres?
  - Paginação por offset ou por cursor?
  - Exclusão física ou *soft delete*?
