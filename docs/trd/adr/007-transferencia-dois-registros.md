# ADR-007 — Transferência como dois registros vinculados

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T5

## Contexto

Uma transferência move dinheiro de uma conta para outra do mesmo usuário. Ela altera o saldo das duas contas, mas não pode contar como receita nem como despesa.

## Decisão

Uma transferência gera **dois registros** em `transactions`, criados na mesma transação de banco:

- um do tipo **`TransferOut`** na conta de origem;
- um do tipo **`TransferIn`** na conta de destino;
- os dois com o mesmo `transfer_pair_id` (um UUID gerado para o par) e sem categoria.

O enum `TransactionType` fica: `Income`, `Expense`, `TransferIn`, `TransferOut`. A API expõe a transferência como uma operação única (`POST /api/transactions/transfer`); editar ou excluir qualquer lado afeta os dois.

## Alternativas consideradas

- **Um registro com `destination_account_id`**: menos linhas, mas o cálculo de saldo e a listagem por conta precisam olhar duas colunas, e cada consulta vira um caso especial.
- **Tabela `transfers` separada**: modelo "puro", mas a listagem de transações de uma conta precisaria juntar duas tabelas.

## Consequências

- ✅ O saldo de qualquer conta é uma soma simples ([ADR-006](006-saldo-calculado.md)).
- ✅ A listagem de uma conta mostra naturalmente "Transferência para Poupança" e "Transferência de Corrente".
- ✅ Dashboard exclui transferências filtrando `type IN ('Income', 'Expense')`.
- ⚠️ É preciso garantir que os dois lados nunca fiquem órfãos: criação, edição e exclusão sempre em transação de banco, cobertas por teste.
