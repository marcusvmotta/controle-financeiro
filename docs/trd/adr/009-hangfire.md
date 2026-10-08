# ADR-009 — Hangfire para jobs agendados

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T8, T9

## Contexto

O sistema tem tarefas que rodam fora de uma requisição:

- **MVP:** reset diário dos dados do usuário demo.
- **Fase 3:** geração de transações recorrentes.
- **Fase 2 (possível):** processamento assíncrono de importações.

## Decisão

Usar **Hangfire** dentro do processo da API, com armazenamento no próprio PostgreSQL (pacote `Hangfire.PostgreSql`, num schema separado `hangfire`). Jobs recorrentes são registrados na inicialização com expressões cron e fuso `America/Sao_Paulo`.

O painel do Hangfire fica em `/hangfire`, protegido (ver [07-observabilidade.md](../07-observabilidade.md)).

## Alternativas consideradas

- **`BackgroundService` nativo**: zero dependências, mas agendamento por cron, retentativas, histórico e garantia de execução única ficam por nossa conta.
- **GitHub Actions agendado chamando um endpoint**: funciona mesmo se a hospedagem "dormir", mas espalha a lógica para fora da aplicação.
- **Quartz.NET**: poderoso, mas sem painel pronto e com configuração mais verbosa.

## Consequências

- ✅ Retentativa automática, histórico de execuções e painel visual, que também é bom para a demo.
- ✅ Um único mecanismo para todos os jobs do projeto.
- ⚠️ `Hangfire.PostgreSql` é mantido pela comunidade, não pelo time do Hangfire. Fixar a versão.
- ⚠️ Em hospedagens gratuitas que "dormem" sem tráfego, jobs podem atrasar. Por isso todo job precisa ser **idempotente** e recuperar execuções atrasadas (ex.: recorrentes geram todas as ocorrências pendentes até hoje).
