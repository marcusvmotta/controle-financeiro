# PRD — Controle Financeiro Pessoal

> **Status:** Rascunho v1 · **Autor:** Marcus Vinicius Motta · **Data:** 2026-10-08
> **Stack:** Angular + ASP.NET Core (C#/.NET) + PostgreSQL

Este PRD está dividido em arquivos menores, um por assunto. Cada módulo funcional tem o seu arquivo com histórias, requisitos, critérios de aceite e uma seção **"Insumos para o TRD"**, que reúne o que o Technical Requirements Document vai precisar detalhar.

## Índice

### Visão do produto
| Arquivo | Conteúdo |
|---------|----------|
| [01-visao-geral.md](01-visao-geral.md) | Visão, problema, objetivos, não-objetivos, personas e métricas de sucesso |
| [02-escopo-e-roadmap.md](02-escopo-e-roadmap.md) | Fases, marcos (M0–M7) e riscos |

### Módulos funcionais
| # | Módulo | Fase | Requisitos |
|---|--------|------|------------|
| 1 | [Autenticação](modulos/01-autenticacao.md) | MVP | RF-01 |
| 2 | [Contas / carteiras](modulos/02-contas.md) | MVP | RF-02 |
| 3 | [Categorias](modulos/03-categorias.md) | MVP | RF-03 |
| 4 | [Transações](modulos/04-transacoes.md) | MVP | RF-04 |
| 5 | [Dashboard](modulos/05-dashboard.md) | MVP | RF-05 |
| 6 | [Usuário demo](modulos/06-usuario-demo.md) | MVP | RF-06 |
| 7 | [Importação de extrato e regras](modulos/07-importacao-extrato.md) | Fase 2 | RF-07, RF-08 |
| 8 | [Orçamento por categoria](modulos/08-orcamento.md) | Fase 2 | RF-09 |
| 9 | [Transações recorrentes](modulos/09-recorrentes.md) | Fase 3 | RF-10 |
| 10 | [Exportação](modulos/10-exportacao.md) | Fase 3 | RF-11 |
| 11 | [Open Finance (sandbox)](modulos/11-open-finance.md) | Fase 3 | RF-12 |

### Requisitos transversais
| Arquivo | Conteúdo |
|---------|----------|
| [03-requisitos-nao-funcionais.md](03-requisitos-nao-funcionais.md) | Segurança, dinheiro, datas, performance, erros, observabilidade, acessibilidade |
| [04-qualidade-e-entrega.md](04-qualidade-e-entrega.md) | Testes, CI/CD, fluxo de Git e README |
| [05-notas-tecnicas.md](05-notas-tecnicas.md) | Stack, arquitetura, modelo de dados e endpoints preliminares (**ponto de partida para o TRD**) |
| [06-questoes-em-aberto.md](06-questoes-em-aberto.md) | Decisões pendentes |

## Convenções

- **Prioridade:** **P0** = obrigatório na fase · **P1** = desejável · **P2** = se sobrar fôlego.
- **IDs:** `RF-xx` = requisito funcional, `RNF-xx` = requisito não-funcional. Use esses IDs nas issues, PRs e no TRD para manter a rastreabilidade.
- **PRD vs. TRD:** o PRD diz **o quê** e **por quê**; o TRD vai dizer **como**. Detalhes de implementação que aparecem aqui são preliminares e serão decididos no TRD.
