# 2. Escopo e roadmap

[← Índice](README.md)

## Fases

| Fase | Tema | Módulos |
|------|------|---------|
| **1 — MVP** | Fundação | [Autenticação](modulos/01-autenticacao.md), [Contas](modulos/02-contas.md), [Categorias](modulos/03-categorias.md), [Transações](modulos/04-transacoes.md), [Dashboard](modulos/05-dashboard.md), [Usuário demo](modulos/06-usuario-demo.md) + Swagger, logs, testes unitários, Docker, CI e deploy |
| **2** | Diferencial | [Importação de extrato e regras](modulos/07-importacao-extrato.md), [Orçamento](modulos/08-orcamento.md) + testes de integração |
| **3** | Extras | [Recorrentes](modulos/09-recorrentes.md), [Exportação](modulos/10-exportacao.md), [Open Finance sandbox](modulos/11-open-finance.md) |

> **Regra:** uma fase só começa quando a anterior estiver com deploy feito, testes passando e README atualizado.

## Marcos

Sem prazo fixo. Os marcos são sequenciais:

1. **M0 — Setup:** monorepo (`/backend`, `/frontend`), docker-compose com Postgres, "hello world" ponta a ponta, CI básico.
2. **M1 — Auth:** cadastro, login, refresh, guard e interceptor.
3. **M2 — Contas e categorias:** CRUDs completos, seed de categorias padrão.
4. **M3 — Transações:** CRUD, transferências, filtros e paginação.
5. **M4 — Dashboard:** cards e gráficos.
6. **M5 — Polimento e deploy:** Swagger, Serilog, health check, usuário demo, README, deploy. 🎉 **MVP publicado**
7. **M6 — Fase 2:** importação OFX/CSV, regras, orçamento, testes de integração.
8. **M7 — Fase 3:** recorrentes, exportação, Open Finance sandbox.

## Dependências entre módulos

```
Autenticação
  └─ Contas ─┬─ Transações ─┬─ Dashboard
  └─ Categorias ┘           ├─ Usuário demo
                            ├─ Importação de extrato (Fase 2)
                            ├─ Orçamento (Fase 2)
                            ├─ Recorrentes (Fase 3)
                            ├─ Exportação (Fase 3)
                            └─ Open Finance (Fase 3)
```

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Escopo crescer e o projeto nunca "terminar" | Faseamento rígido; MVP publicado antes de qualquer feature da Fase 2 |
| Curva de aprendizado dupla (Angular + .NET) | Começar pelo backend com Swagger, depois o frontend consumindo a API pronta |
| Auth com JWT e refresh token é fácil de errar | Seguir a documentação oficial; testes específicos para expiração e rotação. Se travar, fazer login simples primeiro e adicionar refresh depois |
| Hospedagem gratuita mudar as condições | Docker garante portabilidade entre provedores |
| Arredondamento de valores monetários | `decimal` de ponta a ponta; testes com centavos |
