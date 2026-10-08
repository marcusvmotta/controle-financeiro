# Módulo 2 — Contas (técnico)

[← TRD](../README.md) · **PRD:** [RF-02](../../prd/modulos/02-contas.md) · **Decisões:** [ADR-006](../adr/006-saldo-calculado.md), [ADR-008](../adr/008-paginacao-e-exclusao.md)

## Endpoints

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/accounts?includeArchived=false` | Lista contas com saldo atual + saldo total | `200` |
| `GET` | `/api/accounts/{id}` | Detalhe com saldo | `200`, `404` |
| `POST` | `/api/accounts` | Cria | `201`, `400`, `409` (nome duplicado) |
| `PUT` | `/api/accounts/{id}` | Atualiza nome, tipo, saldo inicial, cor, ícone | `200`, `400`, `404`, `409` |
| `DELETE` | `/api/accounts/{id}` | Exclui **somente se não tiver transações** | `204`, `404`, `409` |
| `POST` | `/api/accounts/{id}/archive` | Arquiva | `204`, `404` |
| `POST` | `/api/accounts/{id}/unarchive` | Desarquiva | `204`, `404` |

### Contratos

```json
// POST / PUT request
{ "name": "Nubank", "type": "Checking", "initialBalance": 1500.00, "color": "#8A05BE", "icon": "account_balance" }

// GET /api/accounts response
{
  "items": [
    { "id": "0192...", "name": "Nubank", "type": "Checking", "initialBalance": 1500.00,
      "currentBalance": 2310.45, "color": "#8A05BE", "icon": "account_balance", "isArchived": false }
  ],
  "totalBalance": 5120.90
}
```

`totalBalance` soma apenas contas **não arquivadas**. A lista não é paginada: uma pessoa tem poucas contas.

## Cálculo do saldo

Uma única consulta para todas as contas, sem N+1:

```csharp
db.Accounts
  .Where(a => includeArchived || !a.IsArchived)
  .Select(a => new AccountResponse(
      a.Id, a.Name, a.Type, a.InitialBalance,
      a.InitialBalance
        + a.Transactions.Where(t => t.Type == TransactionType.Income || t.Type == TransactionType.TransferIn).Sum(t => t.Amount)
        - a.Transactions.Where(t => t.Type == TransactionType.Expense || t.Type == TransactionType.TransferOut).Sum(t => t.Amount),
      a.Color, a.Icon, a.IsArchived))
```

> Conferir o SQL gerado nos logs: o EF Core deve traduzir para subconsultas agregadas, não carregar as transações na memória.

## Validações

| Campo | Regras |
|-------|--------|
| `name` | obrigatório, 1–60 caracteres, único por usuário (sem diferenciar maiúsculas) |
| `type` | valor válido do enum |
| `initialBalance` | máximo 2 casas decimais; entre −999.999.999,99 e 999.999.999,99 |
| `color` | regex `^#[0-9A-Fa-f]{6}$` |
| `icon` | da lista permitida de ícones (evita valores arbitrários) |

## Regras de negócio

- Editar o saldo inicial de conta com transações **é permitido** (resolve Q1 do PRD): o saldo atual é recalculado naturalmente pelo [ADR-006](../adr/006-saldo-calculado.md). O frontend mostra um aviso antes de salvar.
- Conta arquivada não aparece nos seletores de nova transação, mas suas transações continuam nas listagens e no dashboard.
- Não é possível lançar transação nova em conta arquivada (`422`).

## Frontend

- Página `/accounts`: cards com nome, ícone, cor e saldo atual; total no topo; alternância "mostrar arquivadas".
- Formulário em `mat-dialog` para criar e editar.
- Excluir: se a API responder `409`, oferecer "Arquivar em vez disso".
