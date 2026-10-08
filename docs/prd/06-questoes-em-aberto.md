# 6. Questões em aberto

[← Índice](README.md)

## Produto (resolver no PRD)

| # | Questão | Módulo |
|---|---------|--------|
| Q1 | ~~O usuário pode editar o saldo inicial de uma conta que já tem transações?~~ **Sim**, com aviso na tela ([TRD](../trd/modulos/02-contas.md#regras-de-negócio)) | [Contas](modulos/02-contas.md) |
| Q2 | Tema escuro entra no MVP ou depois? | Transversal |
| Q3 | Recuperação de senha por e-mail entra logo após o MVP? | [Autenticação](modulos/01-autenticacao.md) |
| Q4 | Orçamento definido mês a mês ou recorrente até ser alterado? | [Orçamento](modulos/08-orcamento.md) |
| Q5 | Qual banco usar como referência para os primeiros extratos OFX/CSV? | [Importação](modulos/07-importacao-extrato.md) |

## Técnicas (resolver no TRD)

Resolvidas no [TRD](../trd/README.md#resumo-das-decisões), exceto a T10.

| # | Questão | Módulo | Decisão |
|---|---------|--------|---------|
| T1 | Angular Material ou PrimeNG? | Transversal | [ADR-003](../trd/adr/003-angular-material.md): Angular Material |
| T2 | ASP.NET Core Identity completo ou `PasswordHasher` + tabela própria? | [Autenticação](modulos/01-autenticacao.md) | [ADR-004](../trd/adr/004-identity-e-jwt.md): Identity + JWT próprio |
| T3 | Onde o frontend guarda os tokens (memória + cookie `HttpOnly`, ou `localStorage`)? | [Autenticação](modulos/01-autenticacao.md) | [ADR-005](../trd/adr/005-armazenamento-de-tokens.md): memória + cookie `HttpOnly` |
| T4 | Saldo da conta calculado na hora ou armazenado? | [Contas](modulos/02-contas.md) | [ADR-006](../trd/adr/006-saldo-calculado.md): calculado na consulta |
| T5 | Transferência como dois registros vinculados ou entidade própria? | [Transações](modulos/04-transacoes.md) | [ADR-007](../trd/adr/007-transferencia-dois-registros.md): dois registros vinculados |
| T6 | Paginação por offset ou cursor? *Soft delete* ou exclusão física? | [Transações](modulos/04-transacoes.md) | [ADR-008](../trd/adr/008-paginacao-e-exclusao.md): offset + exclusão física |
| T7 | Biblioteca de gráficos? | [Dashboard](modulos/05-dashboard.md) | [ADR-011](../trd/adr/011-graficos-ng2-charts.md): ng2-charts |
| T8 | Como agendar o reset do usuário demo? | [Usuário demo](modulos/06-usuario-demo.md) | [ADR-009](../trd/adr/009-hangfire.md): job do Hangfire |
| T9 | Hangfire ou `BackgroundService` nativo? | [Recorrentes](modulos/09-recorrentes.md) | [ADR-009](../trd/adr/009-hangfire.md): Hangfire |
| T10 | Onde ficam os dados entre o upload e a confirmação da importação? | [Importação](modulos/07-importacao-extrato.md) | Em aberto, decidir no início da Fase 2 |
