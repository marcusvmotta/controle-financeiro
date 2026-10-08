# ADR-001 — Arquitetura em camadas, sem CQRS/MediatR

**Status:** Aceito · **Data:** 2026-10-08

## Contexto

O projeto é de aprendizado (iniciante em .NET e Angular) e de portfólio. O backend precisa ser organizado e testável sem que a arquitetura vire o maior obstáculo. Muitos templates populares de .NET trazem Clean Architecture completa com CQRS, MediatR, pipelines e Result pattern, o que multiplica arquivos e conceitos.

## Decisão

Monólito com **4 projetos em camadas**: `Api`, `Application`, `Domain` e `Infrastructure`. A lógica de negócio fica em **services** por feature na camada Application, que acessam os dados por uma interface `IAppDbContext` (sem repositórios genéricos). Erros de negócio são **exceções tipadas**, convertidas em ProblemDetails por um handler global.

## Alternativas consideradas

- **Projeto único**: mais simples, mas não demonstra separação de responsabilidades e deixa o domínio acoplado ao EF Core.
- **Clean Architecture + CQRS/MediatR**: popular, mas adiciona indireção sem benefício real neste tamanho de app. Pode virar uma refatoração documentada no futuro.
- **Repository pattern sobre o EF Core**: o `DbContext` já é um Unit of Work e os `DbSet` já são repositórios. Uma camada extra só esconde recursos do EF (projeções, `Include`).
- **Result pattern em vez de exceções**: mais explícito, porém mais código para um iniciante. Exceções + handler global resolvem bem.

## Consequências

- ✅ Estrutura reconhecível por qualquer dev .NET, com regras de negócio testáveis isoladas.
- ✅ Menos arquivos por feature (controller, service, DTOs, validator).
- ⚠️ A camada Application depende de `Microsoft.EntityFrameworkCore` (por causa de `DbSet` na `IAppDbContext`). Aceito conscientemente em troca de simplicidade.
- ⚠️ É preciso disciplina para não colocar regra de negócio nos controllers.
