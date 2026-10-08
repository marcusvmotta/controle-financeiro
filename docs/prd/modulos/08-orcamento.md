# Módulo 8 — Orçamento por categoria

[← Índice](../README.md) · **Fase:** 2 · **Requisito:** RF-09 · **Prioridade:** P0

## Objetivo

Passar de "ver o que gastei" para "planejar o que vou gastar", com limites mensais por categoria.

## Histórias de usuário

- Como **usuário**, quero definir quanto posso gastar por mês em cada categoria.
- Como **usuário**, quero ver rapidamente quais categorias estão perto ou acima do limite.

## Requisitos

- Limite mensal por categoria de despesa.
- Barra de progresso: verde < 80%, amarelo 80–100%, vermelho > 100%.
- Card no dashboard: "categorias acima do orçamento".
- Opção de copiar os limites do mês anterior.

## Critérios de aceite

- [ ] O valor gasto exibido no orçamento bate com o total da categoria no dashboard.
- [ ] Só é possível criar orçamento para categorias de despesa.
- [ ] Um orçamento por categoria por mês.

## Fora de escopo

- Notificações por e-mail ou push ao estourar o limite.
- Orçamento anual ou por período customizado.
- Saldo que "sobra" passando para o mês seguinte (*rollover*).

## Insumos para o TRD

- **Entidade:** `Budget (Id, UserId, CategoryId, Month, Limit)`.
- **Decidir no TRD:**
  - Representação do mês (`DateOnly` no primeiro dia, ou `Year` + `Month`).
  - Orçamento recorrente (vale para todos os meses até mudar) ou definido mês a mês?
