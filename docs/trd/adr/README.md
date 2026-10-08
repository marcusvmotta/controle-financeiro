# Architecture Decision Records

[← TRD](../README.md)

Um ADR registra **uma** decisão técnica: o contexto, o que foi decidido, as alternativas descartadas e as consequências. Eles servem para você lembrar daqui a seis meses por que fez cada escolha, e são o material da seção "Decisões técnicas" do README.

## Regras

- Um arquivo por decisão, numerado em sequência (`NNN-titulo-curto.md`).
- ADR aceito não é reescrito. Se a decisão mudar, crie um ADR novo e marque o antigo como **Substituído por ADR-NNN**.
- Status possíveis: **Proposto**, **Aceito**, **Substituído**, **Descartado**.

## Lista

| ADR | Título | Status |
|-----|--------|--------|
| [001](001-arquitetura-em-camadas.md) | Arquitetura em camadas, sem CQRS/MediatR | Aceito |
| [002](002-postgresql.md) | PostgreSQL como banco de dados | Aceito |
| [003](003-angular-material.md) | Angular Material como biblioteca de UI | Aceito |
| [004](004-identity-e-jwt.md) | ASP.NET Core Identity + JWT próprio | Aceito |
| [005](005-armazenamento-de-tokens.md) | Access token em memória, refresh token em cookie HttpOnly | Aceito |
| [006](006-saldo-calculado.md) | Saldo da conta calculado na consulta | Aceito |
| [007](007-transferencia-dois-registros.md) | Transferência como dois registros vinculados | Aceito |
| [008](008-paginacao-e-exclusao.md) | Paginação por offset e exclusão física de transações | Aceito |
| [009](009-hangfire.md) | Hangfire para jobs agendados | Aceito |
| [010](010-mesmo-dominio-via-proxy.md) | Frontend e API no mesmo domínio via Nginx | Aceito |
| [011](011-graficos-ng2-charts.md) | ng2-charts para gráficos | Aceito |
| [012](012-dinheiro-e-datas.md) | Representação de dinheiro e datas | Aceito |

## Modelo

```markdown
# ADR-NNN — Título

**Status:** Proposto · **Data:** AAAA-MM-DD

## Contexto
Qual problema ou força levou a essa decisão?

## Decisão
O que foi decidido, em uma ou duas frases afirmativas.

## Alternativas consideradas
- **Opção X**: por que não.

## Consequências
- ✅ O que fica melhor.
- ⚠️ O que fica pior ou exige cuidado.
```
