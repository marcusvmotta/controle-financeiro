# Módulo 11 — Open Finance (sandbox)

[← Índice](../README.md) · **Fase:** 3 · **Requisito:** RF-12 · **Prioridade:** P2

## Objetivo

Demonstrar integração com uma API externa real (autenticação, consentimento e sincronização), **apenas em ambiente sandbox**. Não é objetivo usar com contas bancárias reais.

## Histórias de usuário

- Como **avaliador**, quero ver que o sistema sabe se conectar a um agregador de dados bancários.
- Como **usuário (sandbox)**, quero conectar um banco de teste e ver as transações aparecerem.

## Requisitos

- Integração com o ambiente sandbox de um agregador (ex.: Pluggy).
- Conectar uma conta de teste, sincronizar transações e reaproveitar a deduplicação e as regras do [módulo de importação](07-importacao-extrato.md).
- Deixar explícito no README e na interface que é somente sandbox.

## Critérios de aceite

- [ ] Sincronizar duas vezes não duplica transações.
- [ ] Credenciais da API do agregador ficam só em variáveis de ambiente.
- [ ] A funcionalidade pode ser desligada por configuração.

## Fora de escopo

- Uso em produção com bancos reais (exige contrato e custos com o agregador).
- Iniciação de pagamentos.

## Insumos para o TRD

- **Decidir no TRD:**
  - Qual agregador e quais as condições atuais do sandbox dele.
  - Sincronização sob demanda (botão) ou via webhook / job.
  - Como isolar a integração (interface `IBankDataProvider`) para trocar de provedor.
