# 1. Visão geral

[← Índice](README.md)

## Visão

Aplicação web multiusuário para controle de finanças pessoais. O usuário registra receitas e despesas em suas contas (corrente, poupança, carteira etc.), organiza tudo em categorias e acompanha **para onde o dinheiro está indo** por meio de um dashboard com gráficos.

O projeto tem **duplo objetivo**:

1. **Produto:** ser um app de finanças pessoais funcional e agradável de usar.
2. **Portfólio:** demonstrar domínio de Angular, ASP.NET Core e boas práticas de engenharia (arquitetura em camadas, testes, Docker, CI/CD, deploy, observabilidade).

> Nota sobre o objetivo 2: toda decisão de escopo prioriza **qualidade sobre quantidade**. Uma feature pronta, testada e documentada vale mais que três pela metade.

## Problema

Pessoas perdem a noção de onde gastam o dinheiro. Planilhas exigem disciplina e não dão visão rápida; apps de banco mostram só uma conta por vez. O usuário quer responder em segundos:

- Quanto entrou e quanto saiu este mês?
- Em quais categorias estou gastando mais?
- Qual o saldo de cada conta e o total?
- Estou dentro do orçamento que defini? *(Fase 2)*

## Objetivos

- Permitir cadastro/login seguro e isolamento total de dados entre usuários.
- Registrar transações (receita, despesa, transferência) em múltiplas contas.
- Categorizar transações e visualizar gastos por categoria e por período.
- Ter uma **demo online** com usuário de demonstração pré-populado.
- Rodar localmente com **um único comando** (`docker compose up`).

## Não-objetivos (fora de escopo, por enquanto)

- Investimentos, ações, cripto ou cálculo de rentabilidade.
- Multi-moeda (tudo em BRL).
- App mobile nativo (a web será responsiva).
- Contas compartilhadas entre usuários (família/casal).
- Open Finance em produção (apenas sandbox, na Fase 3, opcional).
- Funcionalidades de empresa (notas fiscais, contas a receber de clientes).
- Fatura e parcelamento de cartão de crédito.

## Personas

**Ana, 28 anos, analista (usuária final).** Recebe salário em uma conta, usa outra para reserva e um pouco de dinheiro vivo. No fim do mês sempre se pergunta "para onde foi meu dinheiro?". Quer algo rápido de lançar e um gráfico que mostre a resposta.

**Avaliador técnico (persona secundária).** Recrutador ou tech lead que abre o repositório no GitHub. Precisa, em poucos minutos: entender o projeto pelo README, ver a demo online, rodar localmente e ver código organizado e testado.

## Métricas de sucesso

Como é um projeto de portfólio, o sucesso é medido pela **qualidade percebida**:

| Métrica | Meta |
|---------|------|
| Tempo para rodar localmente após o clone | < 5 min, com um comando |
| Demo online acessível | Disponível, com usuário demo funcionando |
| CI | Verde na `main` |
| Fluxo principal (login demo → ver dashboard → lançar despesa → ver o gráfico mudar) | < 1 min, sem erros |
| Bugs conhecidos abertos no MVP | 0 críticos |
