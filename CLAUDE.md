# CLAUDE.md

Instruções para o Claude Code neste repositório. São lidas automaticamente em qualquer máquina.

## Sobre o projeto

Sistema web de controle financeiro pessoal, feito como **projeto de aprendizado e portfólio**.

- **Stack:** Angular 22 (standalone, signals, zoneless) · ASP.NET Core .NET 10 · EF Core + PostgreSQL · Docker + Nginx · GitHub Actions.
- **Documentação (ler antes de implementar):**
  - [docs/prd](docs/prd/README.md): o quê e por quê (módulos, requisitos, critérios de aceite).
  - [docs/trd](docs/trd/README.md): como (arquitetura, modelo de dados, API, segurança). Decisões em [docs/trd/adr](docs/trd/adr/README.md).
  - [docs/aprendizado](docs/aprendizado/README.md): diário de aprendizado por marco.
- Se a implementação divergir do TRD, **atualize o TRD** no mesmo PR. ADRs aceitos não são reescritos: adicione uma nota ou crie um ADR novo.

## Como trabalhar com o usuário

- **O usuário é iniciante em Angular e .NET e quer aprender com o projeto.** Explique em português, de forma curta e didática, o que cada passo faz e **por quê**, ligando ao ADR ou TRD correspondente. Explique um conceito na primeira vez que ele aparece, sem repetir depois.
- A cada marco, crie ou atualize o arquivo do marco em `docs/aprendizado/` com os conceitos usados e **exercícios práticos**.
- Seja honesto sobre o que foi e o que não foi testado.

## Fluxo de Git

- Nunca commitar direto na `main`. Uma branch por marco ou feature (`feat/...`, `docs/...`, `fix/...`).
- **Pode fazer commit, push e abrir PR sem perguntar.** Depois, revise o próprio PR (links, consistência, testes). O GitHub não deixa o autor aprovar, então registre a validação como comentário de review (`gh pr review --comment`).
- **Pergunte antes de fazer merge.**
- Commits em inglês, no padrão Conventional Commits (`feat:`, `fix:`, `docs:`...). PRs com descrição em português.
- PRs empilhados: só fazer o merge do segundo depois que a base dele mudar para `main`.

## Convenções

- Documentação em **português**; código, nomes de tabelas, endpoints e commits em **inglês**.
- Backend: camadas `Api → Application → Domain`, `Infrastructure → Application/Domain` (ADR-001). Domain sem dependências externas (teste de arquitetura garante).
- Dinheiro: `decimal` / `numeric(18,2)`. Datas de competência: `DateOnly`. Horário: `TimeProvider` (nunca `DateTime.Now`).
- Versões de pacotes NuGet só em `backend/Directory.Packages.props`.
- `TreatWarningsAsErrors` está ligado: o build precisa ter 0 avisos.

## Comandos

```bash
docker compose up --build                            # tudo: http://localhost:8080 (Swagger em /swagger)
docker compose up db                                 # só o banco, para desenvolvimento
cd backend && dotnet watch --project src/FinanceControl.Api   # API em http://localhost:5000
cd backend && dotnet test
cd frontend && npm start                             # http://localhost:4200 (proxy para a API)
cd frontend && npx ng lint && npx ng test --watch=false && npx ng build
```

## Status atual

- Concluídos: PRD, TRD e **M0 (setup)** (PR #4).
- Próximo: **M1 (autenticação)**, ver [docs/trd/modulos/01-autenticacao.md](docs/trd/modulos/01-autenticacao.md).
- Em aberto: T10 (dados temporários da importação, Fase 2). Avaliar uma biblioteca de asserções de código aberto (FluentAssertions v8 mudou de licença).

> Atualize esta seção ao concluir cada marco.
