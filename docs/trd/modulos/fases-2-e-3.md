# Módulos das Fases 2 e 3 (preliminar)

[← TRD](../README.md)

> ⚠️ Decisões **preliminares**. Cada módulo ganha seu próprio arquivo técnico, com endpoints e contratos, quando a fase começar. Registrar aqui só o que já influencia o MVP (para não fechar portas).

## Importação de extrato e regras: Fase 2 · [PRD](../../prd/modulos/07-importacao-extrato.md)

**Já decidido:**
- CSV com **CsvHelper**. OFX: avaliar biblioteca existente no início da fase; se não houver uma boa e mantida, escrever um parser próprio (OFX 1.x é SGML, e o parser é um bom desafio de portfólio).
- Deduplicação: nova coluna `transactions.external_id` (o `FITID` do OFX, ou um hash de conta + data + valor + descrição normalizada no CSV) com índice **único parcial** `(account_id, external_id) WHERE external_id IS NOT NULL`.
- Encoding: detectar UTF-8 / ISO-8859-1; aceitar decimal com vírgula e datas `dd/MM/yyyy`.
- Upload limitado a 5 MB; arquivo processado em memória, nunca salvo em disco público.
- Transações importadas sem regra correspondente vão para a categoria "Outros" do tipo correspondente (mantém a constraint de categoria obrigatória).

**Em aberto: T10.** Onde ficam os dados entre o upload e a confirmação?
- Opção A: tabela `import_rows` com status `Pending` (sobrevive a reinício; precisa de limpeza).
- Opção B: o backend devolve a pré-visualização e o frontend reenvia as linhas confirmadas (sem estado no servidor; payload maior).

**Novas tabelas:** `imports (id, user_id, account_id, file_name, format, status, total_rows, imported_rows, duplicate_rows, created_at)` e `category_rules (id, user_id, pattern, category_id, priority, created_at)`.

## Orçamento: Fase 2 · [PRD](../../prd/modulos/08-orcamento.md)

- Tabela `budgets (id, user_id, category_id, month date, limit numeric(18,2))`, `month` sempre no dia 1; único `(user_id, category_id, month)`.
- Gasto do mês calculado na consulta, reaproveitando a agregação do dashboard.
- Pendente (Q4 do PRD): orçamento por mês ou recorrente até mudar.

## Transações recorrentes: Fase 3 · [PRD](../../prd/modulos/09-recorrentes.md)

- Tabela `recurring_transactions (id, user_id, account_id, category_id, type, amount, description, frequency, day_of_month?, start_date, end_date?, next_run_date, is_active)`.
- Transações geradas recebem `recurring_transaction_id` (nova coluna nullable) + índice único `(recurring_transaction_id, date)` para garantir idempotência.
- Job Hangfire diário gera **todas** as ocorrências pendentes até hoje (recupera dias em que o servidor "dormiu").
- Dia 31 em meses curtos → último dia do mês.

## Exportação: Fase 3 · [PRD](../../prd/modulos/10-exportacao.md)

- Excel com **ClosedXML**; PDF com **QuestPDF** (verificar a licença: gratuita para uso pessoal e projetos abaixo de um limite de receita, confirmar na época).
- Geração síncrona com streaming do arquivo (`FileStreamResult`); reaproveita o mesmo filtro da listagem.

## Open Finance (sandbox): Fase 3 · [PRD](../../prd/modulos/11-open-finance.md)

- Interface `IBankDataProvider` na Application; implementação do agregador escolhido na Infrastructure.
- Sincronização sob demanda (botão), reaproveitando deduplicação e regras da importação.
- Habilitado por `OpenFinance__Enabled`; credenciais somente em variáveis de ambiente.

## O que o MVP precisa respeitar desde já

- Não criar índice único que impeça transações iguais no mesmo dia (usuários lançam "Uber 15,00" duas vezes no mesmo dia legitimamente).
- Manter a lógica de filtro por período num único método reutilizável (dashboard, listagem, orçamento e exportação usam o mesmo).
- Deixar Hangfire configurado desde o MVP (já usado pelo reset do demo).
