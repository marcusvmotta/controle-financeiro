# PRD — Controle Financeiro Pessoal

> **Status:** Rascunho v1 · **Autor:** Marcus Vinicius Motta · **Data:** 2026-10-08
> **Stack:** Angular + ASP.NET Core (C#/.NET) + PostgreSQL

---

## 1. Visão geral

Aplicação web multiusuário para controle de finanças pessoais. O usuário registra receitas e despesas em suas contas (corrente, poupança, carteira etc.), organiza tudo em categorias e acompanha **para onde o dinheiro está indo** por meio de um dashboard com gráficos.

O projeto tem **duplo objetivo**:

1. **Produto:** ser um app de finanças pessoais funcional e agradável de usar.
2. **Portfólio:** demonstrar domínio de Angular, ASP.NET Core e boas práticas de engenharia (arquitetura em camadas, testes, Docker, CI/CD, deploy, observabilidade).

> Nota sobre o objetivo 2: toda decisão de escopo prioriza **qualidade sobre quantidade**. Uma feature pronta, testada e documentada vale mais que três pela metade.

## 2. Problema

Pessoas perdem a noção de onde gastam o dinheiro. Planilhas exigem disciplina e não dão visão rápida; apps de banco mostram só uma conta por vez. O usuário quer responder em segundos:

- Quanto entrou e quanto saiu este mês?
- Em quais categorias estou gastando mais?
- Qual o saldo de cada conta e o total?
- Estou dentro do orçamento que defini? *(Fase 2)*

## 3. Objetivos e não-objetivos

### Objetivos
- Permitir cadastro/login seguro e isolamento total de dados entre usuários.
- Registrar transações (receita, despesa, transferência) em múltiplas contas.
- Categorizar transações e visualizar gastos por categoria e por período.
- Ter uma **demo online** com usuário de demonstração pré-populado.
- Rodar localmente com **um único comando** (`docker compose up`).

### Não-objetivos (fora de escopo, por enquanto)
- Investimentos, ações, cripto ou cálculo de rentabilidade.
- Multi-moeda (tudo em BRL no MVP).
- App mobile nativo (a web será responsiva).
- Contas compartilhadas entre usuários (família/casal).
- Open Finance em produção (apenas sandbox, na Fase 3, opcional).
- Funcionalidades de empresa (notas fiscais, contas a receber de clientes).

## 4. Persona

**Ana, 28 anos, analista.** Recebe salário em uma conta, usa outra para reserva e um pouco de dinheiro vivo. No fim do mês sempre se pergunta "para onde foi meu dinheiro?". Quer algo rápido de lançar e um gráfico que mostre a resposta.

**Avaliador técnico (persona secundária).** Recrutador ou tech lead que abre o repositório no GitHub. Precisa, em poucos minutos: entender o projeto pelo README, ver a demo online, rodar localmente e ver código organizado e testado.

## 5. Escopo por fases

| Fase | Tema | Conteúdo |
|------|------|----------|
| **1 — MVP** | Fundação | Auth, contas, categorias, transações, dashboard, Swagger, logs, testes unitários, Docker, CI, deploy |
| **2** | Diferencial | Importação de extrato OFX/CSV com regras de categorização, orçamento por categoria, testes de integração |
| **3** | Extras | Transações recorrentes (background job), exportação PDF/Excel, Open Finance (sandbox, opcional) |

> **Regra:** uma fase só começa quando a anterior estiver com deploy feito, testes passando e README atualizado.

---

## 6. Requisitos funcionais

Prioridade: **P0** = obrigatório na fase · **P1** = desejável · **P2** = se sobrar fôlego.

### Fase 1 — MVP

#### RF-01 Autenticação (P0)
- Cadastro com nome, e-mail e senha.
- Login retorna **access token JWT** (curta duração, ~15 min) + **refresh token** (longa duração, armazenado no banco, rotacionado a cada uso).
- Logout invalida o refresh token.
- Senha com hash (ASP.NET Core Identity ou `PasswordHasher`).

**Critérios de aceite**
- [ ] E-mail duplicado retorna `409 Conflict`.
- [ ] Senha fraca (< 8 caracteres, sem número ou letra) é rejeitada com mensagem clara.
- [ ] Rotas protegidas retornam `401` sem token válido.
- [ ] O frontend renova o access token automaticamente (HTTP interceptor) sem deslogar o usuário.
- [ ] Um usuário **nunca** acessa dados de outro (teste automatizado garante isso).

#### RF-02 Contas / carteiras (P0)
- CRUD de contas: nome, tipo (`Checking`, `Savings`, `Cash`, `CreditCard`*, `Other`), saldo inicial, cor/ícone.
- Saldo atual = saldo inicial + soma das transações.
- Arquivar conta (não some do histórico, apenas some das listas ativas).

*\* No MVP, cartão de crédito é tratado como conta comum (saldo negativo). Fatura e parcelamento ficam fora de escopo.*

**Critérios de aceite**
- [ ] Saldo exibido bate com a soma das transações.
- [ ] Conta com transações não pode ser excluída, apenas arquivada.
- [ ] Tela mostra saldo por conta e saldo total consolidado.

#### RF-03 Categorias (P0)
- Categorias separadas por tipo: receita ou despesa.
- Novo usuário recebe um **conjunto padrão** (Alimentação, Moradia, Transporte, Saúde, Lazer, Educação, Salário, Outros…).
- CRUD de categorias, com nome, cor e ícone.

**Critérios de aceite**
- [ ] Categoria em uso não pode ser excluída sem reatribuir as transações (ou é arquivada).
- [ ] Não é possível usar categoria de despesa numa receita e vice-versa.

#### RF-04 Transações (P0)
- Tipos: **Receita**, **Despesa**, **Transferência** (entre contas do próprio usuário).
- Campos: descrição, valor, data, conta, categoria (exceto transferência), observação opcional.
- Listagem com **paginação**, **filtros** (período, conta, categoria, tipo) e **busca** por descrição.
- Editar e excluir.

**Critérios de aceite**
- [ ] Valor deve ser > 0 (o tipo define se entra ou sai).
- [ ] Transferência gera dois lançamentos vinculados; editar ou excluir um afeta ambos.
- [ ] Transferência não entra nos totais de receita e despesa do dashboard.
- [ ] Filtro padrão: mês atual.
- [ ] Lista responde em < 500 ms com 10 mil transações (índices corretos).

#### RF-05 Dashboard (P0)
Para o período selecionado (padrão: mês atual):
- Cards: **total de receitas**, **total de despesas**, **saldo do período**, **saldo total das contas**.
- Gráfico de **despesas por categoria** (rosca ou barras).
- Gráfico de **evolução mensal** receitas × despesas (últimos 6–12 meses).
- Lista das **últimas 5 transações**.

**Critérios de aceite**
- [ ] Os números do dashboard batem com a listagem filtrada pelo mesmo período.
- [ ] Estado vazio amigável para usuário novo ("Cadastre sua primeira transação").

#### RF-06 Usuário demo (P0)
- Seed com usuário `demo` e ~6 meses de transações realistas.
- Botão "Entrar como demo" na tela de login (só no ambiente de demonstração).
- Dados do demo são **resetados periodicamente** (ex.: diariamente).

### Fase 2 — Diferencial

#### RF-07 Importação de extrato (P0)
- Upload de arquivo **OFX** e **CSV** (com mapeamento de colunas para o CSV).
- Tela de **pré-visualização** antes de confirmar: o usuário revisa, ajusta categorias e desmarca linhas.
- **Detecção de duplicatas** (mesma conta + data + valor + descrição normalizada, ou `FITID` do OFX).
- Histórico de importações.

**Critérios de aceite**
- [ ] Importar o mesmo arquivo duas vezes não duplica transações.
- [ ] Arquivo inválido retorna erro claro, sem importar nada (operação atômica).
- [ ] Limite de tamanho de arquivo (ex.: 5 MB).

#### RF-08 Regras de categorização (P1)
- Regras do tipo "se a descrição contém `UBER` → Transporte".
- Aplicadas automaticamente na importação, e opcionalmente na criação manual.
- Sugestão: ao recategorizar manualmente, oferecer "criar regra para casos parecidos?".

#### RF-09 Orçamento por categoria (P0)
- Limite mensal por categoria de despesa.
- Barra de progresso (verde < 80%, amarelo 80–100%, vermelho > 100%).
- Card no dashboard: "categorias acima do orçamento".

### Fase 3 — Extras

#### RF-10 Transações recorrentes (P0)
- Definir recorrência: mensal, semanal ou anual, com data de início e fim opcional.
- **Background job** (Hangfire ou `BackgroundService`) gera as transações na data.
- Visualizar e cancelar recorrências.

#### RF-11 Exportação (P1)
- Exportar transações filtradas em **Excel** (ClosedXML) e um **relatório mensal em PDF** (QuestPDF).

#### RF-12 Open Finance — sandbox (P2)
- Integração com o ambiente sandbox de um agregador (ex.: Pluggy) apenas para demonstrar consumo de API externa, OAuth e sincronização.
- Fica explícito no README que é somente sandbox.

---

## 7. Requisitos não-funcionais

| Área | Requisito |
|------|-----------|
| **Segurança** | HTTPS; senhas com hash; JWT com expiração curta; refresh token rotacionado; CORS restrito ao domínio do frontend; validação de entrada em todas as rotas; nenhum segredo no repositório (usar variáveis de ambiente / `dotnet user-secrets`). |
| **Isolamento** | Toda query filtrada por `UserId` (EF Core *global query filter*), coberta por teste. |
| **Dinheiro** | Valores como `decimal(18,2)` no C# e `numeric(18,2)` no Postgres. **Nunca** `float`/`double`. |
| **Datas** | Armazenar em UTC (`timestamptz`); exibir no fuso do usuário (padrão `America/Sao_Paulo`). Transação usa `DateOnly` para a data de competência. |
| **Performance** | Listagens paginadas; índices em `(UserId, Date)`, `(UserId, CategoryId)`, `(AccountId)`. |
| **Erros** | Respostas de erro no padrão **ProblemDetails** (RFC 9457); erros de validação com campo e mensagem. |
| **Observabilidade** | **Serilog** com logs estruturados (JSON em produção) e correlation ID por requisição; endpoint `/health` (health checks com verificação do banco). |
| **Documentação** | **Swagger/OpenAPI** com autenticação JWT configurada no Swagger UI. |
| **Acessibilidade** | Navegação por teclado, labels em formulários, contraste adequado. |
| **Responsividade** | Utilizável em telas a partir de 360 px de largura. |
| **i18n** | Interface em PT-BR; formatação de moeda `R$ 1.234,56` e datas `dd/MM/yyyy`. |

---

## 8. Arquitetura

### 8.1 Stack

| Camada | Tecnologia |
|--------|------------|
| Frontend | **Angular** (versão estável mais recente), standalone components, **signals**, Reactive Forms, Angular Router com guards, HttpClient com interceptors |
| UI | **Angular Material** (ou PrimeNG) + gráficos com **ng2-charts / Chart.js** |
| Backend | **ASP.NET Core Web API** em **.NET 10 (LTS)**, Controllers |
| ORM | **Entity Framework Core** + Npgsql, migrations versionadas |
| Banco | **PostgreSQL** |
| Validação | **FluentValidation** |
| Auth | JWT Bearer + refresh token |
| Logs | **Serilog** |
| Testes back | **xUnit**, **FluentAssertions**, **NSubstitute**; integração com **WebApplicationFactory** + **Testcontainers** (Fase 2) |
| Testes front | Jest ou Vitest (o padrão da versão do Angular) + Angular Testing Library |
| Infra | **Docker** + **docker-compose**, **GitHub Actions** |

### 8.2 Organização do backend (camadas simples)

```
backend/
├── src/
│   ├── FinanceControl.Api/            # Controllers, middlewares, Program.cs, Swagger
│   ├── FinanceControl.Application/    # Services, DTOs, validators, interfaces
│   ├── FinanceControl.Domain/         # Entidades, enums, regras de negócio puras
│   └── FinanceControl.Infrastructure/ # EF Core DbContext, migrations, repositórios, JWT
└── tests/
    ├── FinanceControl.UnitTests/
    └── FinanceControl.IntegrationTests/
```

Dependências: `Api → Application → Domain` e `Infrastructure → Application/Domain`. O Domain não depende de nada.

> Decisão consciente: **sem CQRS/MediatR** no início. Camadas com services são suficientes e mais fáceis de aprender. Pode virar uma refatoração documentada no futuro (o que também conta pontos no portfólio).

### 8.3 Organização do frontend

```
frontend/src/app/
├── core/          # auth service, interceptors, guards, layout
├── shared/        # componentes reutilizáveis, pipes (moeda), models
└── features/
    ├── auth/
    ├── dashboard/
    ├── accounts/
    ├── categories/
    ├── transactions/
    └── budgets/   # Fase 2
```

Rotas das features com **lazy loading**.

### 8.4 Modelo de dados (MVP)

```
User          (Id, Name, Email, PasswordHash, CreatedAt)
RefreshToken  (Id, UserId, TokenHash, ExpiresAt, RevokedAt, ReplacedByTokenId)
Account       (Id, UserId, Name, Type, InitialBalance, Color, Icon, IsArchived, CreatedAt)
Category      (Id, UserId, Name, Type[Income|Expense], Color, Icon, IsArchived)
Transaction   (Id, UserId, AccountId, CategoryId?, Type[Income|Expense|Transfer],
               Amount, Date, Description, Notes?, TransferPairId?, CreatedAt, UpdatedAt)
```

Fase 2: `Budget (Id, UserId, CategoryId, Month, Limit)`, `CategoryRule (Id, UserId, Pattern, CategoryId)`, `Import (Id, UserId, AccountId, FileName, Status, CreatedAt)` e `Transaction.ExternalId` para deduplicação.
Fase 3: `RecurringTransaction (Id, UserId, ..., Frequency, StartDate, EndDate?, NextRunDate)`.

### 8.5 Endpoints principais (MVP)

```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
GET    /api/me

GET    /api/accounts                POST /api/accounts
GET    /api/accounts/{id}           PUT  /api/accounts/{id}     DELETE /api/accounts/{id}

GET    /api/categories              POST /api/categories
PUT    /api/categories/{id}         DELETE /api/categories/{id}

GET    /api/transactions?from=&to=&accountId=&categoryId=&type=&search=&page=&pageSize=
POST   /api/transactions            POST /api/transactions/transfer
PUT    /api/transactions/{id}       DELETE /api/transactions/{id}

GET    /api/dashboard/summary?from=&to=
GET    /api/dashboard/expenses-by-category?from=&to=
GET    /api/dashboard/monthly-evolution?months=12

GET    /health
```

### 8.6 Deploy (demo pública)

Opções com camada gratuita ou de baixo custo (verificar as condições atuais de cada uma na hora do deploy):
- **Frontend:** Vercel, Netlify, Cloudflare Pages ou GitHub Pages.
- **API (container Docker):** Render, Fly.io, Railway ou Azure App Service.
- **Banco:** Neon ou Supabase (Postgres gerenciado).

---

## 9. Qualidade e entrega

### 9.1 Testes
- **Unitários (Fase 1):** regras de domínio (cálculo de saldo, transferência, validações) e services.
- **Integração (Fase 2):** endpoints reais com Postgres via Testcontainers, incluindo o teste de **isolamento entre usuários**.
- **Frontend:** services, guards, interceptors e componentes principais (formulário de transação).
- Meta indicativa: ≥ 70% de cobertura na camada Application/Domain. Cobertura é consequência, não objetivo.

### 9.2 CI/CD (GitHub Actions)
- A cada push e PR: build do backend e do frontend, testes e lint.
- Na `main`: build das imagens Docker e deploy automático.
- Badges no README: build, testes e link da demo.

### 9.3 Fluxo de trabalho no Git
- Branch por feature (`feat/transactions-crud`), PRs mesmo trabalhando sozinho, **Conventional Commits**.
- Issues e um GitHub Project (kanban) com as histórias deste PRD. Isso também mostra organização.

### 9.4 README (vitrine do projeto)
- GIF ou screenshots do dashboard.
- Link da demo + credenciais do usuário demo.
- Diagrama da arquitetura.
- "Como rodar": `docker compose up` e mais nada.
- Seção **"Decisões técnicas"** (por que Postgres, por que camadas, por que `decimal`…), que é onde o avaliador vê o seu raciocínio.

---

## 10. Métricas de sucesso

Como é um projeto de portfólio, o sucesso é medido pela **qualidade percebida**:

| Métrica | Meta |
|---------|------|
| Tempo para rodar localmente após o clone | < 5 min, com um comando |
| Demo online acessível | Disponível, com usuário demo funcionando |
| CI | Verde na `main` |
| Fluxo principal (login demo → ver dashboard → lançar despesa → ver o gráfico mudar) | < 1 min, sem erros |
| Bugs conhecidos abertos no MVP | 0 críticos |

---

## 11. Roadmap / marcos

Sem prazo fixo. Os marcos são sequenciais:

1. **M0 — Setup:** monorepo (`/backend`, `/frontend`), docker-compose com Postgres, "hello world" ponta a ponta, CI básico.
2. **M1 — Auth:** cadastro, login, refresh, guard e interceptor.
3. **M2 — Contas e categorias:** CRUDs completos, seed de categorias padrão.
4. **M3 — Transações:** CRUD, transferências, filtros e paginação.
5. **M4 — Dashboard:** cards e gráficos.
6. **M5 — Polimento e deploy:** Swagger, Serilog, health check, usuário demo, README, deploy. 🎉 **MVP publicado**
7. **M6 — Fase 2:** importação OFX/CSV, regras, orçamento, testes de integração.
8. **M7 — Fase 3:** recorrentes, exportação, Open Finance sandbox.

---

## 12. Riscos

| Risco | Mitigação |
|-------|-----------|
| Escopo crescer e o projeto nunca "terminar" | Faseamento rígido; MVP publicado antes de qualquer feature da Fase 2 |
| Curva de aprendizado dupla (Angular + .NET) | Começar pelo backend com Swagger, depois o frontend consumindo a API pronta |
| Auth com JWT e refresh token é fácil de errar | Seguir a documentação oficial; testes específicos para expiração e rotação |
| Hospedagem gratuita mudar as condições | Docker garante portabilidade entre provedores |
| Arredondamento de valores monetários | `decimal` de ponta a ponta; testes com centavos |

## 13. Questões em aberto

- Angular Material ou PrimeNG? (Material é mais "oficial"; PrimeNG tem mais componentes prontos, como tabelas e gráficos.)
- Tema escuro no MVP ou depois?
- O usuário pode editar o saldo inicial de uma conta que já tem transações?
- Hangfire (com painel visual, bom para demo) ou `BackgroundService` nativo (mais simples) na Fase 3?
- Qual banco usar como referência para o primeiro parser OFX/CSV (depende dos extratos que você tem à mão)?
