# Módulo 4 — Transações (técnico)

[← TRD](../README.md) · **PRD:** [RF-04](../../prd/modulos/04-transacoes.md) · **Decisões:** [ADR-007](../adr/007-transferencia-dois-registros.md), [ADR-008](../adr/008-paginacao-e-exclusao.md), [ADR-012](../adr/012-dinheiro-e-datas.md)

## Endpoints

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/transactions` | Lista paginada com filtros | `200`, `400` |
| `GET` | `/api/transactions/{id}` | Detalhe | `200`, `404` |
| `POST` | `/api/transactions` | Cria receita ou despesa | `201`, `400`, `404`, `422` |
| `PUT` | `/api/transactions/{id}` | Edita receita ou despesa | `200`, `400`, `404`, `422` |
| `DELETE` | `/api/transactions/{id}` | Exclui; se for transferência, exclui os dois lados | `204`, `404` |
| `POST` | `/api/transactions/transfer` | Cria transferência | `201`, `400`, `404`, `422` |
| `PUT` | `/api/transactions/transfer/{pairId}` | Edita transferência (os dois lados) | `200`, `400`, `404`, `422` |

### Listagem

`GET /api/transactions?from=2026-10-01&to=2026-10-31&accountId=&categoryId=&type=Expense&search=mercado&page=1&pageSize=20`

| Parâmetro | Padrão | Observação |
|-----------|--------|------------|
| `from`, `to` | primeiro e último dia do mês atual | inclusivos; `to >= from`; intervalo máximo de 366 dias |
| `accountId`, `categoryId` | — | opcionais |
| `type` | — | `Income`, `Expense` ou `Transfer` (este último cobre `TransferIn` e `TransferOut`) |
| `search` | — | `ILIKE '%termo%'` na descrição, 2–100 caracteres |
| `page`, `pageSize` | 1, 20 | máximo 100 |

Ordenação: `date DESC, created_at DESC`.

```json
// item da resposta (dentro de PagedResult)
{
  "id": "0192...",
  "type": "Expense",
  "amount": 87.90,
  "date": "2026-10-05",
  "description": "Mercado",
  "notes": null,
  "account": { "id": "0192...", "name": "Nubank" },
  "category": { "id": "0192...", "name": "Alimentação", "color": "#4CAF50", "icon": "restaurant" },
  "transfer": null
}
```

Para transferências, `category` é `null` e `transfer` traz `{ "pairId": "...", "counterpartAccount": { "id": "...", "name": "Poupança" } }`.

### Criar receita ou despesa

```json
{ "type": "Expense", "amount": 87.90, "date": "2026-10-05", "description": "Mercado",
  "accountId": "0192...", "categoryId": "0192...", "notes": null }
```

### Criar ou editar transferência

```json
{ "fromAccountId": "0192...", "toAccountId": "0192...", "amount": 500.00,
  "date": "2026-10-05", "description": "Reserva do mês", "notes": null }
```

Resposta: `{ "pairId": "...", "outgoing": { ...transação... }, "incoming": { ...transação... } }`.

## Validações

| Campo | Regras |
|-------|--------|
| `type` | `Income` ou `Expense` no endpoint comum (transferência tem endpoint próprio) |
| `amount` | > 0, máximo 2 casas decimais, até 999.999.999,99 |
| `date` | obrigatória; entre 1900-01-01 e hoje + 5 anos (aceita lançamentos futuros planejados) |
| `description` | obrigatória, 1–200 caracteres, sem espaços nas pontas |
| `notes` | opcional, até 1000 caracteres |
| `fromAccountId` / `toAccountId` | diferentes entre si |

## Regras de negócio (service)

1. Conta e categoria precisam existir e ser do usuário (query filter → `404` se não).
2. Ao **criar**, ou ao **mudar** a conta ou a categoria de uma transação, a nova conta e a nova categoria não podem estar arquivadas (`422`).
3. `category.Type` precisa bater com `transaction.Type` (`422`).
4. Transferência: as duas contas do usuário, não arquivadas, diferentes entre si.
5. Criar, editar e excluir transferência sempre numa **transação de banco** (`SaveChangesAsync` único para os dois lados).
6. `PUT /api/transactions/{id}` numa transferência → `422` orientando a usar o endpoint de transferência.
7. Editar uma transação que **já está** numa conta ou categoria arquivada é permitido (para corrigir histórico), desde que não a mova para outra conta ou categoria arquivada (regra 2).

## Performance

- Meta do PRD: listagem em < 500 ms com 10 mil transações.
- Atendida pelo índice `(user_id, date DESC)` + projeção direta para o DTO + `AsNoTracking`.
- A busca `ILIKE` sem índice é aceitável porque já filtra por usuário e período. Se precisar, adicionar `pg_trgm` com índice GIN na descrição.
- Validar a meta com um teste de integração que insere 10 mil transações (rodado no CI, sem asserção rígida de tempo; apenas registra a duração).

## Frontend

- Página `/transactions`:
  - barra de filtros (período com atalhos "Este mês", "Mês passado", "Últimos 30 dias"; conta; categoria; tipo; busca com *debounce* de 300 ms);
  - filtros sincronizados com a URL;
  - `mat-table` + `mat-paginator` no desktop; lista de cards no celular;
  - totais do período filtrado no topo (receitas, despesas, saldo).
- Botão flutuante "Nova transação" abre um `mat-dialog` com abas **Despesa / Receita / Transferência**. O seletor de categoria mostra só as do tipo escolhido.
- Excluir pede confirmação; para transferência, avisa que os dois lados serão excluídos.
