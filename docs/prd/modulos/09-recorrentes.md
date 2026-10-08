# Módulo 9 — Transações recorrentes

[← Índice](../README.md) · **Fase:** 3 · **Requisito:** RF-10 · **Prioridade:** P0

## Objetivo

Automatizar lançamentos que se repetem (salário, aluguel, assinaturas) e demonstrar processamento em background.

## Histórias de usuário

- Como **usuário**, quero cadastrar meu salário uma vez e vê-lo lançado automaticamente todo mês.
- Como **usuário**, quero ver e cancelar as recorrências ativas (ex.: cancelei uma assinatura).

## Requisitos

- Definir recorrência: **mensal**, **semanal** ou **anual**, com data de início e data de fim opcional.
- Um **background job** gera as transações na data prevista.
- Visualizar, editar e cancelar recorrências.

## Critérios de aceite

- [ ] Se o job ficar parado por alguns dias, ao voltar ele gera as transações atrasadas, sem duplicar.
- [ ] Recorrência mensal no dia 31 gera no último dia dos meses mais curtos.
- [ ] Cancelar uma recorrência não apaga as transações já geradas.

## Fora de escopo

- Lembretes de contas a pagar (notificação sem gerar transação).
- Recorrências com valor variável.

## Insumos para o TRD

- **Entidade:** `RecurringTransaction (Id, UserId, AccountId, CategoryId, Type, Amount, Description, Frequency, StartDate, EndDate?, NextRunDate)`.
- **Decidir no TRD:**
  - **Hangfire** (painel visual, bom para demo) ou `BackgroundService` nativo (mais simples)?
  - Garantia de idempotência (não gerar a mesma ocorrência duas vezes).
  - Comportamento em hospedagem gratuita que "dorme" quando ociosa.
