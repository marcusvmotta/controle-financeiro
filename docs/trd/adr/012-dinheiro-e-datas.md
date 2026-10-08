# ADR-012 — Representação de dinheiro e datas

**Status:** Aceito · **Data:** 2026-10-08

## Contexto

`0.1 + 0.2 != 0.3` em ponto flutuante: usar `double` para dinheiro gera erros de centavos. E datas de transação têm dois sentidos diferentes: "quando o gasto aconteceu" (um dia do calendário) e "quando o registro foi criado" (um instante no tempo).

## Decisão

**Dinheiro**
- C#: `decimal`. Postgres: `numeric(18,2)`. Nunca `float`/`double`.
- Valores sempre positivos; o tipo da transação define se entra ou sai.
- No JSON, valores trafegam como número (`1234.56`). Toda soma é feita **no backend**; o frontend só exibe.
- Arredondamento, quando necessário, com `MidpointRounding.ToEven` (padrão do .NET), aplicado só na exibição.

**Datas**
- **Data de competência** (quando o gasto aconteceu): C# `DateOnly`, Postgres `date`, JSON `"2026-10-08"`. Sem hora e sem fuso.
- **Instantes** (`created_at`, `updated_at`, expiração de tokens): C# `DateTimeOffset` em UTC, Postgres `timestamptz`, JSON ISO 8601 com `Z`.
- "Mês atual" e "hoje" são calculados no fuso `America/Sao_Paulo` por um serviço `IClock`, para poderem ser fixados nos testes.

## Alternativas consideradas

- **Guardar centavos como inteiro (`long`)**: elimina o problema de ponto flutuante, mas todo código precisa lembrar de dividir por 100. Com `decimal` nativo no C# e no Postgres, não compensa.
- **`DateTime` para tudo**: ambíguo quanto ao fuso, causa transações "mudando de dia" dependendo de onde o servidor roda.

## Consequências

- ✅ Sem erros de centavos e sem transações mudando de dia por fuso horário.
- ⚠️ Números JSON viram `number` (ponto flutuante) no JavaScript. Seguro aqui porque o frontend não faz contas, só formata. Se um dia fizer, usar strings no JSON.
