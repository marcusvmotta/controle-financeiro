# 3. Convenções da API

[← TRD](README.md)

Os endpoints de cada módulo estão nos arquivos de [`modulos/`](README.md#módulos-detalhamento-técnico). Aqui ficam as regras que valem para todos.

## Geral

- Prefixo `/api`. Sem versionamento no MVP: há um único cliente (o próprio frontend), publicado junto com a API. Se surgir um segundo cliente, adotar `/api/v1`.
- Recursos no plural e em inglês: `/api/accounts`, `/api/transactions`.
- JSON em `camelCase`; enums como **string** (`"Expense"`, não `1`).
- Todas as rotas exigem autenticação, exceto `register`, `login`, `refresh`, `logout`, `demo`, `config` (todas em `/api/auth`) e `/health/*`. `/swagger` e `/hangfire` têm regras próprias por ambiente ([07-observabilidade.md](07-observabilidade.md#painel-do-hangfire)).
- Documentação via **Swagger/OpenAPI** em `/swagger` (habilitado em dev e na demo), com botão "Authorize" para colar o JWT.

## Formatos

| Tipo | Formato JSON | Exemplo |
|------|--------------|---------|
| Dinheiro | número com até 2 casas | `1234.56` |
| Data de competência | `yyyy-MM-dd` | `"2026-10-08"` |
| Instante | ISO 8601 em UTC | `"2026-10-08T14:30:00Z"` |
| ID | UUID | `"0192f3a4-..."` |

Detalhes e motivos no [ADR-012](adr/012-dinheiro-e-datas.md).

## Status HTTP

| Status | Uso |
|--------|-----|
| `200 OK` | Leitura e atualização com corpo de resposta |
| `201 Created` | Criação; header `Location` com a URL do recurso |
| `204 No Content` | Exclusão, logout, arquivamento |
| `400 Bad Request` | Validação de entrada |
| `401 Unauthorized` | Sem token, token inválido ou expirado |
| `404 Not Found` | Não existe **ou é de outro usuário** (nunca `403`, para não revelar que o recurso existe) |
| `409 Conflict` | Conflito de estado (e-mail já usado, conta com transações) |
| `422 Unprocessable Entity` | Regra de negócio violada |
| `429 Too Many Requests` | Rate limit dos endpoints de autenticação |
| `500` | Erro inesperado |

## Paginação

Requisição: `?page=1&pageSize=20` (`page` começa em 1; `pageSize` padrão 20, máximo 100). Decisão no [ADR-008](adr/008-paginacao-e-exclusao.md).

Resposta:
```json
{
  "items": [ /* ... */ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 137,
  "totalPages": 7
}
```

## Filtros e ordenação

- Filtros por query string com nomes explícitos: `from`, `to`, `accountId`, `categoryId`, `type`, `search`.
- Períodos são **inclusivos** nas duas pontas (`from <= date <= to`).
- Ordenação fixa por endpoint no MVP (transações: `date DESC, created_at DESC`). Ordenação configurável fica para depois, se fizer falta.

## Erros

Todas as respostas de erro seguem **ProblemDetails** (RFC 9457), com `traceId` para cruzar com os logs:

```json
{
  "type": "https://httpstatuses.io/404",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Conta não encontrada.",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

Erros de validação (`400`) incluem o campo `errors`, com as mensagens por campo em **português** (exibidas direto na tela):

```json
{
  "title": "Um ou mais campos são inválidos",
  "status": 400,
  "errors": {
    "amount": ["O valor deve ser maior que zero."],
    "date": ["A data é obrigatória."]
  },
  "traceId": "..."
}
```

## Mapa de endpoints (MVP)

| Módulo | Endpoints |
|--------|-----------|
| [Autenticação](modulos/01-autenticacao.md) | `POST /api/auth/register` · `POST /api/auth/login` · `POST /api/auth/refresh` · `POST /api/auth/logout` · `POST /api/auth/demo` · `GET /api/auth/config` · `GET /api/me` |
| [Contas](modulos/02-contas.md) | `GET/POST /api/accounts` · `GET/PUT/DELETE /api/accounts/{id}` · `POST /api/accounts/{id}/archive` · `POST /api/accounts/{id}/unarchive` |
| [Categorias](modulos/03-categorias.md) | `GET/POST /api/categories` · `PUT/DELETE /api/categories/{id}` · `POST /api/categories/{id}/archive` · `POST /api/categories/{id}/unarchive` |
| [Transações](modulos/04-transacoes.md) | `GET/POST /api/transactions` · `GET/PUT/DELETE /api/transactions/{id}` · `POST /api/transactions/transfer` · `PUT /api/transactions/transfer/{pairId}` |
| [Dashboard](modulos/05-dashboard.md) | `GET /api/dashboard/summary` · `GET /api/dashboard/expenses-by-category` · `GET /api/dashboard/monthly-evolution` |
| Infra | `GET /health/live` · `GET /health/ready` |
