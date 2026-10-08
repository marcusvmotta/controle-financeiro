# Módulo 6 — Usuário demo (técnico)

[← TRD](../README.md) · **PRD:** [RF-06](../../prd/modulos/06-usuario-demo.md) · **Decisões:** [ADR-009](../adr/009-hangfire.md)

## Visão geral

- Um **único usuário demo compartilhado** (`is_demo = true`), criado na inicialização se `Demo:Enabled=true`.
- Login sem senha via `POST /api/auth/demo` → mesma resposta do login comum.
- Dados resetados diariamente às 03:00 (America/Sao_Paulo) pelo job `reset-demo-user`.

> Um demo isolado por visitante seria mais "limpo", mas multiplicaria dados e exigiria limpeza de usuários temporários. Com reset diário, o compartilhado é suficiente; a tela de login avisa que os dados são públicos e resetados todo dia.

## Geração dos dados (`DemoDataSeeder`)

- Código C# em `Infrastructure/Persistence/Seeds/`, **determinístico** (`new Random(42)`): o mesmo dia gera sempre os mesmos dados, facilitando depuração.
- **Datas relativas a hoje:** os últimos 6 meses, incluindo o mês atual até a data de hoje. O dashboard nunca fica vazio.
- Conteúdo:

| Conta | Tipo | Saldo inicial |
|-------|------|---------------|
| Banco Principal | Checking | 2.000,00 |
| Reserva | Savings | 5.000,00 |
| Carteira | Cash | 150,00 |

| Recorrência | Exemplos |
|-------------|----------|
| Mensais fixas | Salário (dia 5), aluguel (dia 10), internet, streaming, academia |
| Variáveis frequentes | Mercado (3–5×/mês), transporte por app (8–15×/mês), restaurantes, farmácia |
| Esporádicas | Presentes, manutenção, compras online |
| Transferências | Reserva mensal (Banco Principal → Reserva), saque para a carteira |

Valores com pequena variação aleatória (± 15%) para os gráficos parecerem reais. O resultado deve ser um saldo positivo e algumas categorias "pesando" mais, para o gráfico contar uma história.

## Reset (`ResetDemoUserJob`)

1. Em uma transação de banco: apaga transações, contas, categorias e refresh tokens do usuário demo (`IgnoreQueryFilters()`, permitido aqui por ser job de infraestrutura).
2. Recria categorias padrão e roda o `DemoDataSeeder`.
3. Idempotente: rodar duas vezes seguidas produz o mesmo estado.
4. Também roda na inicialização da API se o usuário demo não existir.

## Restrições do usuário demo

- Não pode trocar senha, e-mail ou excluir a conta (`403`).
- Pode criar, editar e excluir contas, categorias e transações normalmente: experimentar é o objetivo.
- Rate limit de escrita mais baixo para o demo (ex.: 60 escritas/min), para evitar abuso.

## Configuração

| Variável | Padrão |
|----------|--------|
| `Demo__Enabled` | `false` |
| `Demo__ResetCron` | `0 3 * * *` |

## Frontend

- `GET /api/auth/config` → `{ demoEnabled }` decide se o botão "Entrar como demo" aparece.
- Quando `user.isDemo`, a toolbar mostra um selo "Modo demo · dados resetados diariamente".
