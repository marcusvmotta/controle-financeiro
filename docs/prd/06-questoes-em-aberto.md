# 6. Questões em aberto

[← Índice](README.md)

## Produto (resolver no PRD)

| # | Questão | Módulo |
|---|---------|--------|
| Q1 | O usuário pode editar o saldo inicial de uma conta que já tem transações? | [Contas](modulos/02-contas.md) |
| Q2 | Tema escuro entra no MVP ou depois? | Transversal |
| Q3 | Recuperação de senha por e-mail entra logo após o MVP? | [Autenticação](modulos/01-autenticacao.md) |
| Q4 | Orçamento definido mês a mês ou recorrente até ser alterado? | [Orçamento](modulos/08-orcamento.md) |
| Q5 | Qual banco usar como referência para os primeiros extratos OFX/CSV? | [Importação](modulos/07-importacao-extrato.md) |

## Técnicas (resolver no TRD)

| # | Questão | Módulo |
|---|---------|--------|
| T1 | Angular Material ou PrimeNG? | Transversal |
| T2 | ASP.NET Core Identity completo ou `PasswordHasher` + tabela própria? | [Autenticação](modulos/01-autenticacao.md) |
| T3 | Onde o frontend guarda os tokens (memória + cookie `HttpOnly`, ou `localStorage`)? | [Autenticação](modulos/01-autenticacao.md) |
| T4 | Saldo da conta calculado na hora ou armazenado? | [Contas](modulos/02-contas.md) |
| T5 | Transferência como dois registros vinculados ou entidade própria? | [Transações](modulos/04-transacoes.md) |
| T6 | Paginação por offset ou cursor? *Soft delete* ou exclusão física? | [Transações](modulos/04-transacoes.md) |
| T7 | Biblioteca de gráficos? | [Dashboard](modulos/05-dashboard.md) |
| T8 | Como agendar o reset do usuário demo? | [Usuário demo](modulos/06-usuario-demo.md) |
| T9 | Hangfire ou `BackgroundService` nativo? | [Recorrentes](modulos/09-recorrentes.md) |
| T10 | Onde ficam os dados entre o upload e a confirmação da importação? | [Importação](modulos/07-importacao-extrato.md) |
