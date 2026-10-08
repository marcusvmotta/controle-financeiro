# Módulo 2 — Contas / carteiras

[← Índice](../README.md) · **Fase:** MVP · **Requisito:** RF-02 · **Prioridade:** P0

## Objetivo

Representar os lugares onde o dinheiro do usuário está (conta corrente, poupança, dinheiro vivo…) e mostrar o saldo de cada um e o total.

## Histórias de usuário

- Como **usuário**, quero cadastrar minhas contas para registrar de onde sai e para onde vai cada transação.
- Como **usuário**, quero ver o saldo de cada conta e o saldo total consolidado.
- Como **usuário**, quero arquivar uma conta que não uso mais sem perder o histórico.

## Requisitos

- CRUD de contas: nome, tipo (`Checking`, `Savings`, `Cash`, `CreditCard`*, `Other`), saldo inicial, cor e ícone.
- Saldo atual = saldo inicial + soma das transações da conta.
- Arquivar conta: ela some das listas ativas, mas continua no histórico e nos relatórios.

*\* Cartão de crédito é tratado como conta comum (saldo negativo). Fatura e parcelamento ficam fora de escopo.*

## Critérios de aceite

- [ ] Saldo exibido bate com a soma das transações.
- [ ] Conta com transações não pode ser excluída, apenas arquivada.
- [ ] Tela mostra saldo por conta e saldo total consolidado.

## Fora de escopo

- Fatura de cartão, data de fechamento e parcelamento.
- Contas em outras moedas.
- Conciliação com saldo real do banco.

## Insumos para o TRD

- **Entidade:** `Account (Id, UserId, Name, Type, InitialBalance, Color, Icon, IsArchived, CreatedAt)`.
- **Endpoints preliminares:** `GET/POST /api/accounts`, `GET/PUT/DELETE /api/accounts/{id}`.
- **Decidir no TRD:**
  - Saldo calculado na hora (soma das transações) ou armazenado e atualizado a cada transação? Trade-off entre simplicidade e performance.
  - O usuário pode editar o saldo inicial de uma conta que já tem transações? (ver [questões em aberto](../06-questoes-em-aberto.md))
