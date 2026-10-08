# Módulo 7 — Importação de extrato e regras de categorização

[← Índice](../README.md) · **Fase:** 2 · **Requisitos:** RF-07 (P0), RF-08 (P1)

## Objetivo

Eliminar a digitação manual trazendo as transações direto do extrato do banco. É o **diferencial técnico** do projeto: leitura de arquivos, deduplicação e categorização automática.

## Histórias de usuário

- Como **usuário**, quero enviar o extrato do meu banco e ter as transações cadastradas sem digitar uma por uma.
- Como **usuário**, quero revisar o que vai ser importado antes de confirmar.
- Como **usuário**, quero que o sistema não duplique transações se eu importar o mesmo extrato duas vezes.
- Como **usuário**, quero que o sistema categorize sozinho as transações que se repetem (ex.: Uber → Transporte).

## RF-07 — Importação de extrato (P0)

- Upload de arquivo **OFX** e **CSV** (com mapeamento de colunas para o CSV).
- Tela de **pré-visualização** antes de confirmar: o usuário revisa, ajusta categorias e desmarca linhas.
- **Detecção de duplicatas** (mesma conta + data + valor + descrição normalizada, ou `FITID` do OFX).
- Histórico de importações.

**Critérios de aceite**
- [ ] Importar o mesmo arquivo duas vezes não duplica transações.
- [ ] Arquivo inválido retorna erro claro, sem importar nada (operação atômica).
- [ ] Limite de tamanho de arquivo (ex.: 5 MB).
- [ ] Linhas marcadas como duplicatas aparecem destacadas na pré-visualização.

## RF-08 — Regras de categorização (P1)

- Regras do tipo "se a descrição contém `UBER` → Transporte".
- Aplicadas automaticamente na importação e, opcionalmente, na criação manual.
- Ao recategorizar manualmente, oferecer "criar regra para casos parecidos?".

**Critérios de aceite**
- [ ] Regras são avaliadas em ordem de prioridade definida pelo usuário.
- [ ] Transação sem regra correspondente fica como "Sem categoria" para revisão.

## Fora de escopo

- Leitura de extrato em PDF.
- Categorização com IA / machine learning.

## Insumos para o TRD

- **Entidades:** `Import (Id, UserId, AccountId, FileName, Status, CreatedAt)`, `CategoryRule (Id, UserId, Pattern, CategoryId, Priority)`, `Transaction.ExternalId` (para deduplicação).
- **Decidir no TRD:**
  - Biblioteca de parsing de OFX (existe pronta em .NET ou parser próprio?) e de CSV (CsvHelper).
  - Fluxo em duas etapas: upload → pré-visualização (dados temporários) → confirmação. Onde ficam os dados entre as etapas?
  - Processamento síncrono ou assíncrono (fila / background job)?
  - Encoding dos arquivos dos bancos brasileiros (UTF-8 vs. ISO-8859-1) e formatos de data e decimal (`1.234,56`).
  - Bancos de referência para os primeiros testes (depende dos extratos disponíveis).
