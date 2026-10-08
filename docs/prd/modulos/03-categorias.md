# Módulo 3 — Categorias

[← Índice](../README.md) · **Fase:** MVP · **Requisito:** RF-03 · **Prioridade:** P0

## Objetivo

Classificar receitas e despesas para que o usuário entenda **em que** gasta e **de onde** vem o dinheiro.

## Histórias de usuário

- Como **usuário novo**, quero começar com categorias prontas para não precisar configurar nada antes de lançar a primeira despesa.
- Como **usuário**, quero criar, renomear e personalizar (cor, ícone) as minhas categorias.

## Requisitos

- Categorias separadas por tipo: **receita** ou **despesa**.
- Novo usuário recebe um **conjunto padrão** (Alimentação, Moradia, Transporte, Saúde, Lazer, Educação, Salário, Outros…).
- CRUD de categorias com nome, cor e ícone.

## Critérios de aceite

- [ ] Categoria em uso não pode ser excluída sem reatribuir as transações (ou é arquivada).
- [ ] Não é possível usar categoria de despesa numa receita e vice-versa.
- [ ] Usuário recém-cadastrado já vê as categorias padrão.

## Fora de escopo

- Subcategorias (ex.: Alimentação → Restaurante). *Candidato a evolução futura.*
- Tags livres.

## Insumos para o TRD

- **Entidade:** `Category (Id, UserId, Name, Type[Income|Expense], Color, Icon, IsArchived)`.
- **Endpoints preliminares:** `GET/POST /api/categories`, `PUT/DELETE /api/categories/{id}`.
- **Decidir no TRD:**
  - Categorias padrão copiadas para cada usuário no cadastro, ou categorias "globais" compartilhadas + categorias do usuário?
  - Onde fica a lista padrão (seed no código, tabela de templates)?
