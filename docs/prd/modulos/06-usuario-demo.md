# Módulo 6 — Usuário demo

[← Índice](../README.md) · **Fase:** MVP · **Requisito:** RF-06 · **Prioridade:** P0

## Objetivo

Deixar o **avaliador técnico** ver o sistema funcionando com dados realistas em menos de 10 segundos, sem precisar se cadastrar.

## Histórias de usuário

- Como **avaliador**, quero clicar em "Entrar como demo" e já ver um dashboard cheio de dados.
- Como **dono do projeto**, quero que a bagunça feita por visitantes seja desfeita automaticamente.

## Requisitos

- Seed com usuário `demo` e ~6 meses de transações realistas (salário, aluguel, mercado, transporte, lazer…), distribuídas em 2–3 contas.
- Botão **"Entrar como demo"** na tela de login (só no ambiente de demonstração).
- Dados do demo **resetados periodicamente** (ex.: diariamente).
- As datas do seed são relativas ao dia atual, para que o "mês atual" nunca fique vazio.

## Critérios de aceite

- [ ] Botão demo só aparece quando habilitado por configuração.
- [ ] Após o reset, o dashboard do demo volta ao estado original.
- [ ] Visitantes demo não conseguem alterar a senha nem excluir o usuário demo.

## Insumos para o TRD

- **Decidir no TRD:**
  - Como gerar o seed (código C# com dados determinísticos, ou arquivo JSON/SQL)?
  - Como agendar o reset (job agendado na API, GitHub Actions agendado, cron do provedor)?
  - Um único usuário demo compartilhado ou um demo isolado por sessão?
  - Feature flag / variável de ambiente para habilitar o modo demo.
