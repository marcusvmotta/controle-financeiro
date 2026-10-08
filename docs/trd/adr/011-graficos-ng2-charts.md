# ADR-011 — ng2-charts para gráficos

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T7

## Contexto

O dashboard precisa de um gráfico de rosca (despesas por categoria) e um de barras (receitas × despesas por mês). O Angular Material não tem gráficos.

## Decisão

Usar **ng2-charts**, um wrapper Angular do **Chart.js**.

## Alternativas consideradas

- **ngx-echarts (Apache ECharts)**: mais recursos e animações, porém bundle maior e configuração mais extensa.
- **ApexCharts**: visual bonito pronto, mas licença com restrições comerciais nas versões recentes. Verificar antes de qualquer uso.
- **D3 direto**: controle total, mas é desproporcional para dois gráficos.

## Consequências

- ✅ Chart.js é a biblioteca de gráficos mais conhecida; documentação e exemplos abundantes.
- ✅ Os dois tipos de gráfico necessários são nativos.
- ⚠️ As cores dos gráficos precisam seguir o tema do Material (claro/escuro) manualmente.
