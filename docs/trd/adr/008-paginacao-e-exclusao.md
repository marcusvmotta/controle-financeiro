# ADR-008 — Paginação por offset e exclusão física de transações

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T6

## Contexto

A listagem de transações precisa de paginação, e o usuário precisa poder excluir lançamentos errados. Contas e categorias já têm "arquivamento" definido no PRD.

## Decisão

- **Paginação por offset**: parâmetros `page` (a partir de 1) e `pageSize` (padrão 20, máximo 100). A resposta inclui `totalCount` e `totalPages`.
- **Exclusão física** de transações (`DELETE` de verdade).
- Contas e categorias **não são excluídas** se tiverem transações; são arquivadas (`is_archived`).

## Alternativas consideradas

- **Paginação por cursor**: escala melhor com milhões de linhas, mas não permite ir para a página N nem mostrar o total. Não combina com o `mat-paginator` e é exagero para o volume deste app.
- **Soft delete (`deleted_at`)**: permite "desfazer" e auditoria, mas adiciona um filtro global a mais e complica os índices únicos.

## Consequências

- ✅ Combina diretamente com o `mat-paginator` do Angular Material.
- ✅ Modelo de dados mais simples.
- ⚠️ Excluir não tem volta. O frontend pede confirmação antes de excluir.
- ⚠️ `COUNT(*)` a cada página tem custo; com o filtro por usuário e o índice `(user_id, date)` é aceitável.
