# FinanceControl

[![CI](https://github.com/marcusvmotta/controle-financeiro/actions/workflows/ci.yml/badge.svg)](https://github.com/marcusvmotta/controle-financeiro/actions/workflows/ci.yml)

Sistema web de controle financeiro pessoal: registre receitas e despesas em várias contas, organize por categorias e descubra **para onde o seu dinheiro está indo**.

> 🚧 Em construção. Marco atual: **M0 (setup)**. Veja o [roadmap](docs/prd/02-escopo-e-roadmap.md).

## Stack

| Camada | Tecnologia |
|--------|------------|
| Frontend | Angular 22 (standalone, signals) |
| Backend | ASP.NET Core (.NET 10), EF Core |
| Banco | PostgreSQL |
| Infra | Docker, Nginx, GitHub Actions |

## Como rodar

**Pré-requisito:** [Docker](https://www.docker.com/products/docker-desktop/).

```bash
docker compose up --build
```

- App: http://localhost:8080
- Swagger (documentação da API): http://localhost:8080/swagger

### Desenvolvimento (com hot reload)

Pré-requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download) e [Node.js 22](https://nodejs.org/).

```bash
# 1. Banco de dados
docker compose up db

# 2. API (http://localhost:5000, abre o Swagger)
cd backend && dotnet watch --project src/FinanceControl.Api

# 3. Frontend (http://localhost:4200)
cd frontend && npm install && npm start
```

### Testes

```bash
cd backend && dotnet test
cd frontend && npx ng test --watch=false
```

## Documentação

- [PRD](docs/prd/README.md): o que o produto faz e por quê.
- [TRD](docs/trd/README.md): como ele é construído.
- [Decisões técnicas (ADRs)](docs/trd/adr/README.md): por que cada tecnologia foi escolhida.
- [Diário de aprendizado](docs/aprendizado/README.md): o que foi aprendido em cada marco.

## Estrutura

```
├── backend/          # API ASP.NET Core (camadas: Api, Application, Domain, Infrastructure)
├── frontend/         # SPA Angular + Nginx
├── docs/             # PRD, TRD, ADRs e diário de aprendizado
└── docker-compose.yml
```
