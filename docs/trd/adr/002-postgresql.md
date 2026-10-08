# ADR-002 — PostgreSQL como banco de dados

**Status:** Aceito · **Data:** 2026-10-08

## Contexto

O app é relacional por natureza (usuários, contas, categorias, transações com chaves estrangeiras) e precisa de agregações (somas por categoria e por mês). O banco precisa rodar em Docker localmente e ter hospedagem gratuita ou barata para a demo.

## Decisão

Usar **PostgreSQL** com **EF Core + Npgsql**, nomes de tabelas e colunas em `snake_case` (pacote `EFCore.NamingConventions`). A versão da imagem Docker é fixada no M0.

## Alternativas consideradas

- **SQL Server**: muito usado no mercado .NET brasileiro, mas a hospedagem gratuita é rara e a imagem Docker é pesada.
- **SQLite**: ótimo para protótipos, mas tem limitações de concorrência e tipos (não tem `numeric` real) e não representa um ambiente de produção.
- **MongoDB / NoSQL**: os dados são claramente relacionais; não há ganho.

## Consequências

- ✅ Gratuito, leve em Docker, com Postgres gerenciado gratuito disponível (Neon, Supabase).
- ✅ Recursos úteis para fases futuras: `pg_trgm` para busca, índices parciais, `date_trunc` para agregações.
- ✅ O Hangfire também pode usar o mesmo banco (`Hangfire.PostgreSql`).
- ⚠️ Menos exemplos de .NET + Postgres na internet do que de .NET + SQL Server.
