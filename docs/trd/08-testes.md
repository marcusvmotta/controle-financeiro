# 8. Estratégia de testes

[← TRD](README.md)

## Pirâmide

| Camada | Ferramentas | O que cobre | Quando roda |
|--------|-------------|-------------|-------------|
| **Unitários (backend)** | xUnit, FluentAssertions, NSubstitute | Regras de domínio, validators, services com dependências simuladas | Todo push (CI) |
| **Integração (backend)** | `WebApplicationFactory`, **Testcontainers** (PostgreSQL real), Respawn | Endpoints HTTP de ponta a ponta com banco real: auth, isolamento, transferências, agregações do dashboard | Todo push (CI) |
| **Unitários (frontend)** | Runner padrão do Angular CLI (Vitest nas versões recentes; confirmar no M0) + Angular Testing Library | Stores, interceptors, guards, pipes, formulários | Todo push (CI) |
| **E2E** | Playwright | Fluxo principal: login demo → dashboard → lançar despesa → ver o gráfico mudar | *P2, depois do MVP* |

> Testes de integração com **Postgres real** (e não SQLite ou banco em memória) pegam erros de SQL, constraints e tipos (`numeric`, `date`) que um banco falso esconde.

## Convenções

- Nome do teste no padrão `Metodo_Cenario_ResultadoEsperado`, por exemplo `CreateTransfer_SameAccount_ThrowsBusinessRule`.
- Estrutura **Arrange / Act / Assert** visível no corpo do teste.
- Um container Postgres por execução da suíte de integração (fixture compartilhada); o **Respawn** limpa as tabelas entre os testes.
- `IClock` fixo nos testes, para "mês atual" e "hoje" serem determinísticos.
- Builders de dados de teste (`TransactionBuilder`, `AccountBuilder`) para não repetir setup.

## Testes obrigatórios (não negociáveis)

Estes testes protegem os requisitos mais críticos do PRD e precisam existir antes do módulo ser considerado pronto:

| # | Teste | Tipo | Protege |
|---|-------|------|---------|
| 1 | Usuário B recebe `404` ao ler, editar ou excluir contas, categorias e transações do usuário A | Integração | RNF-02, isolamento |
| 2 | Usuário B não consegue criar transação na conta ou com a categoria do usuário A | Integração | RNF-02 |
| 3 | Refresh token é rotacionado; reusar o antigo revoga a família inteira | Integração | RF-01 |
| 4 | Rotas protegidas retornam `401` sem token e com token expirado | Integração | RF-01 |
| 5 | Saldo da conta = saldo inicial + entradas − saídas, incluindo transferências | Integração | RF-02, ADR-006 |
| 6 | Transferência cria os dois lados; excluir um exclui os dois; nenhum lado entra no dashboard | Integração | RF-04, ADR-007 |
| 7 | Totais do dashboard batem com a soma da listagem filtrada no mesmo período | Integração | RF-05 |
| 8 | Valores com centavos somam sem erro de arredondamento (ex.: 0,10 + 0,20 = 0,30) | Unitário + integração | ADR-012 |
| 9 | Categoria de despesa em receita (e vice-versa) é rejeitada com `422` | Unitário | RF-03 |
| 10 | `authInterceptor` faz um único refresh para várias requisições `401` simultâneas | Unitário (front) | ADR-005 |

## Cobertura

- Medida com **coverlet** (backend) e o coverage do runner do Angular (frontend), publicada como artefato do CI.
- Meta indicativa: **≥ 70%** em Domain + Application. Não é um "portão" do CI: cobertura é consequência dos testes certos, não objetivo.
