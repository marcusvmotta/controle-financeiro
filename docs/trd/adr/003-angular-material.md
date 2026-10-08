# ADR-003 — Angular Material como biblioteca de UI

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T1

## Contexto

O frontend precisa de formulários, tabelas paginadas e ordenáveis, diálogos, menus, date pickers e snackbars, com acessibilidade razoável e tema claro/escuro, sem que o desenvolvedor (iniciante) precise construir tudo do zero.

## Decisão

Usar **Angular Material** (com o CDK) como biblioteca de componentes, com um tema customizado com as cores do app.

## Alternativas consideradas

- **PrimeNG**: mais componentes prontos (tabela com filtros embutidos, gráficos, upload), mas a API é maior e as atualizações costumam trazer mudanças incompatíveis.
- **Tailwind + componentes próprios**: visual único, mas exige muito mais trabalho, principalmente em acessibilidade (foco, teclado, ARIA).

## Consequências

- ✅ Mantido pelo time do Angular: acompanha as versões do framework e segue boas práticas de acessibilidade.
- ✅ `mat-table` + `mat-paginator` + `mat-sort` cobrem a listagem de transações; `mat-datepicker` com locale pt-BR cobre as datas.
- ✅ Tema escuro (questão Q2 do PRD) fica barato de implementar depois.
- ⚠️ Visual "cara de Google" se não customizar o tema.
- ⚠️ Não traz gráficos; resolvido no [ADR-011](011-graficos-ng2-charts.md).
