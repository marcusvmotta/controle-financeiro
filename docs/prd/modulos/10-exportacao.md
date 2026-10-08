# Módulo 10 — Exportação

[← Índice](../README.md) · **Fase:** 3 · **Requisito:** RF-11 · **Prioridade:** P1

## Objetivo

Permitir que o usuário leve seus dados para fora do sistema (planilha ou relatório para guardar).

## Histórias de usuário

- Como **usuário**, quero exportar as transações filtradas para Excel para fazer minhas próprias análises.
- Como **usuário**, quero baixar um relatório mensal em PDF com o resumo do mês.

## Requisitos

- Exportar a lista de transações **com os filtros atuais** em **Excel (.xlsx)**.
- Gerar **relatório mensal em PDF** com: totais do mês, despesas por categoria e lista de transações.

## Critérios de aceite

- [ ] O arquivo exportado contém exatamente as transações da tela filtrada.
- [ ] Valores e datas no formato brasileiro.
- [ ] Nome do arquivo inclui o período (ex.: `transacoes-2026-10.xlsx`).

## Insumos para o TRD

- **Decidir no TRD:**
  - Bibliotecas: **ClosedXML** (Excel) e **QuestPDF** (PDF). Verificar licenças.
  - Geração síncrona (download direto) ou assíncrona para períodos grandes.
  - Gráfico no PDF: gerado como imagem no servidor ou só tabelas.
