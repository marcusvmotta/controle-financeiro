# Módulo 5 — Dashboard

[← Índice](../README.md) · **Fase:** MVP · **Requisito:** RF-05 · **Prioridade:** P0

## Objetivo

Responder em segundos à pergunta central do produto: **"para onde está indo o meu dinheiro?"**. É também a principal vitrine visual do projeto no README e na demo.

## Histórias de usuário

- Como **usuário**, quero ver quanto entrou e quanto saiu no mês, e o saldo do período.
- Como **usuário**, quero ver em quais categorias estou gastando mais.
- Como **usuário**, quero comparar meus gastos e receitas mês a mês.
- Como **usuário novo**, quero saber o que fazer quando ainda não tenho dados.

## Requisitos

Para o período selecionado (padrão: mês atual):

- Cards: **total de receitas**, **total de despesas**, **saldo do período**, **saldo total das contas**.
- Gráfico de **despesas por categoria** (rosca ou barras).
- Gráfico de **evolução mensal** receitas × despesas (últimos 6–12 meses).
- Lista das **últimas 5 transações**.

## Critérios de aceite

- [ ] Os números do dashboard batem com a listagem de transações filtrada pelo mesmo período.
- [ ] Transferências não entram em receitas nem em despesas.
- [ ] Estado vazio amigável para usuário novo ("Cadastre sua primeira transação").
- [ ] Clicar numa categoria do gráfico leva à lista de transações filtrada por ela *(P1)*.

## Fora de escopo

- Dashboards customizáveis pelo usuário.
- Projeções e previsões de saldo.

## Insumos para o TRD

- **Endpoints preliminares:**
  - `GET /api/dashboard/summary?from=&to=`
  - `GET /api/dashboard/expenses-by-category?from=&to=`
  - `GET /api/dashboard/monthly-evolution?months=12`
- **Decidir no TRD:**
  - Biblioteca de gráficos (ng2-charts/Chart.js, ApexCharts, ou a do PrimeNG).
  - Agregações feitas no banco (`GROUP BY`) via EF Core ou SQL direto.
  - Fuso horário usado para definir "mês atual".
