# Módulo 3 — Categorias (técnico)

[← TRD](../README.md) · **PRD:** [RF-03](../../prd/modulos/03-categorias.md)

## Endpoints

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/categories?type=Expense&includeArchived=false` | Lista (filtro opcional por tipo) | `200` |
| `POST` | `/api/categories` | Cria | `201`, `400`, `409` |
| `PUT` | `/api/categories/{id}` | Atualiza nome, cor, ícone (**tipo não pode mudar**) | `200`, `400`, `404`, `409` |
| `DELETE` | `/api/categories/{id}` | Exclui **somente se não tiver transações** | `204`, `404`, `409` |
| `POST` | `/api/categories/{id}/archive` | Arquiva | `204`, `404` |
| `POST` | `/api/categories/{id}/unarchive` | Desarquiva | `204`, `404` |

```json
// POST request
{ "name": "Pets", "type": "Expense", "color": "#FF8A65", "icon": "pets" }

// response
{ "id": "0192...", "name": "Pets", "type": "Expense", "color": "#FF8A65", "icon": "pets",
  "isArchived": false, "transactionCount": 0 }
```

`transactionCount` permite ao frontend saber, antes de tentar, se a exclusão será possível.

## Regras de negócio

- O **tipo não muda** depois de criada: mudar de despesa para receita invalidaria as transações existentes. Para "mudar", cria-se outra categoria.
- Excluir categoria com transações → `409`. O frontend oferece **arquivar**. Reatribuir transações em lote fica fora do MVP.
- Categorias padrão são cópias do usuário ([02-modelo-de-dados.md](../02-modelo-de-dados.md#seeds)) e podem ser renomeadas, arquivadas ou excluídas como qualquer outra.

## Validações

| Campo | Regras |
|-------|--------|
| `name` | obrigatório, 1–40 caracteres, único por usuário **e tipo** (sem diferenciar maiúsculas) |
| `type` | `Income` ou `Expense` |
| `color`, `icon` | mesmas regras de contas |

## Frontend

- Página `/categories` com duas abas (`mat-tab-group`): Despesas e Receitas.
- Lista com cor, ícone, nome e quantidade de transações.
- Formulário em `mat-dialog` com seletor de cor e de ícone.
