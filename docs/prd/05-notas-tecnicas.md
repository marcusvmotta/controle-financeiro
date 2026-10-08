# 5. Notas técnicas preliminares

[← Índice](README.md)

> ⚠️ Este arquivo é o **ponto de partida para o TRD**. Tudo aqui é proposta inicial, e as decisões finais (com justificativa) serão registradas no Technical Requirements Document.

## Stack proposta

| Camada | Tecnologia |
|--------|------------|
| Frontend | **Angular** (versão estável mais recente), standalone components, **signals**, Reactive Forms, Router com guards, HttpClient com interceptors |
| UI | **Angular Material** ou **PrimeNG** + biblioteca de gráficos |
| Backend | **ASP.NET Core Web API** em **.NET 10 (LTS)**, Controllers |
| ORM | **Entity Framework Core** + Npgsql, migrations versionadas |
| Banco | **PostgreSQL** |
| Validação | **FluentValidation** |
| Auth | JWT Bearer + refresh token |
| Logs | **Serilog** |
| Testes back | **xUnit**, **FluentAssertions**, **NSubstitute**; integração com **WebApplicationFactory** + **Testcontainers** |
| Testes front | Jest ou Vitest (o padrão da versão do Angular) + Angular Testing Library |
| Infra | **Docker** + **docker-compose**, **GitHub Actions** |

## Organização do backend (camadas simples)

```
backend/
├── src/
│   ├── FinanceControl.Api/            # Controllers, middlewares, Program.cs, Swagger
│   ├── FinanceControl.Application/    # Services, DTOs, validators, interfaces
│   ├── FinanceControl.Domain/         # Entidades, enums, regras de negócio puras
│   └── FinanceControl.Infrastructure/ # EF Core DbContext, migrations, repositórios, JWT
└── tests/
    ├── FinanceControl.UnitTests/
    └── FinanceControl.IntegrationTests/
```

Dependências: `Api → Application → Domain` e `Infrastructure → Application/Domain`. O Domain não depende de nada.

> Proposta: **sem CQRS/MediatR** no início. Camadas com services são suficientes e mais fáceis de aprender. Pode virar uma refatoração documentada no futuro.

## Organização do frontend

```
frontend/src/app/
├── core/          # auth service, interceptors, guards, layout
├── shared/        # componentes reutilizáveis, pipes (moeda), models
└── features/
    ├── auth/
    ├── dashboard/
    ├── accounts/
    ├── categories/
    ├── transactions/
    └── budgets/   # Fase 2
```

Rotas das features com **lazy loading**.

## Modelo de dados consolidado

| Entidade | Campos | Fase | Módulo |
|----------|--------|------|--------|
| `User` | Id, Name, Email, PasswordHash, CreatedAt | MVP | [Autenticação](modulos/01-autenticacao.md) |
| `RefreshToken` | Id, UserId, TokenHash, ExpiresAt, RevokedAt, ReplacedByTokenId | MVP | [Autenticação](modulos/01-autenticacao.md) |
| `Account` | Id, UserId, Name, Type, InitialBalance, Color, Icon, IsArchived, CreatedAt | MVP | [Contas](modulos/02-contas.md) |
| `Category` | Id, UserId, Name, Type, Color, Icon, IsArchived | MVP | [Categorias](modulos/03-categorias.md) |
| `Transaction` | Id, UserId, AccountId, CategoryId?, Type, Amount, Date, Description, Notes?, TransferPairId?, ExternalId?, CreatedAt, UpdatedAt | MVP | [Transações](modulos/04-transacoes.md) |
| `Import` | Id, UserId, AccountId, FileName, Status, CreatedAt | 2 | [Importação](modulos/07-importacao-extrato.md) |
| `CategoryRule` | Id, UserId, Pattern, CategoryId, Priority | 2 | [Importação](modulos/07-importacao-extrato.md) |
| `Budget` | Id, UserId, CategoryId, Month, Limit | 2 | [Orçamento](modulos/08-orcamento.md) |
| `RecurringTransaction` | Id, UserId, AccountId, CategoryId, Type, Amount, Description, Frequency, StartDate, EndDate?, NextRunDate | 3 | [Recorrentes](modulos/09-recorrentes.md) |

## Endpoints preliminares (MVP)

```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
GET    /api/me

GET    /api/accounts                POST /api/accounts
GET    /api/accounts/{id}           PUT  /api/accounts/{id}     DELETE /api/accounts/{id}

GET    /api/categories              POST /api/categories
PUT    /api/categories/{id}         DELETE /api/categories/{id}

GET    /api/transactions?from=&to=&accountId=&categoryId=&type=&search=&page=&pageSize=
POST   /api/transactions            POST /api/transactions/transfer
PUT    /api/transactions/{id}       DELETE /api/transactions/{id}

GET    /api/dashboard/summary?from=&to=
GET    /api/dashboard/expenses-by-category?from=&to=
GET    /api/dashboard/monthly-evolution?months=12

GET    /health
```

## Deploy (demo pública)

Opções com camada gratuita ou de baixo custo (verificar as condições atuais de cada uma na hora do deploy):

- **Frontend:** Vercel, Netlify, Cloudflare Pages ou GitHub Pages.
- **API (container Docker):** Render, Fly.io, Railway ou Azure App Service.
- **Banco:** Neon ou Supabase (Postgres gerenciado).

## Sugestão de estrutura para o TRD

Para manter a mesma organização do PRD:

```
docs/trd/
├── README.md                  # índice + diagrama de arquitetura
├── 01-arquitetura.md          # camadas, dependências, padrões
├── 02-stack-e-decisoes.md     # ADRs: por que cada tecnologia
├── 03-modelo-de-dados.md      # diagrama ER, tipos, índices, constraints
├── 04-api.md                  # convenções REST, erros, paginação, versionamento
├── 05-seguranca.md            # auth, tokens, CORS, segredos
├── 06-infra-e-deploy.md       # Docker, CI/CD, ambientes
├── 07-observabilidade.md      # logs, health checks
├── 08-testes.md               # estratégia e ferramentas
└── modulos/                   # detalhes técnicos de cada módulo do PRD
```
