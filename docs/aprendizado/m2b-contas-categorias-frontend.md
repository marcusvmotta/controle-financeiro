# M2b — Contas e categorias (frontend)

[← Diário](README.md) · **Backend:** [M2a](m2a-contas-categorias-backend.md) · **Especificação:** [TRD 05-frontend](../trd/05-frontend.md), [TRD contas](../trd/modulos/02-contas.md), [TRD categorias](../trd/modulos/03-categorias.md)

**Objetivo do marco:** as primeiras telas "de verdade" do app: menu lateral, contas com saldo e categorias por tipo, com criar, editar, arquivar e excluir.

---

## 1. A arquitetura de uma feature

Toda feature segue o mesmo padrão (TRD 05-frontend). Depois que você entende uma, entende todas:

```
features/accounts/
├── data/
│   ├── account.models.ts    # tipos (espelham os DTOs da API)
│   ├── accounts.api.ts      # SÓ HTTP, sem estado
│   └── accounts.store.ts    # estado (signals) + operações
├── components/
│   └── account-dialog.ts    # formulário de criar/editar
└── pages/
    └── accounts-page.ts     # a tela: lê a store, abre diálogos
```

```mermaid
flowchart LR
    Page["AccountsPage"] -->|lê signals| Store["AccountsStore"]
    Page -->|abre| Dialog["AccountDialog"]
    Dialog -->|"save(request)"| Store
    Store --> Api["AccountsApi"] --> API[(API)]
    Store -->|depois de salvar: load()| Api
```

## 2. Conceitos

### Store por página (`providers: [AccountsStore]`)
- A store **não** é `providedIn: 'root'`; ela é fornecida pela página.
- Consequência: cada vez que você abre a tela, nasce uma store nova, com estado limpo. Quando sai da tela, ela é destruída. Não sobra lixo de uma visita para a outra.

### Estados da tela: carregando, vazio, erro e lista
```html
@switch (store.status()) {
  @case ('loading') { <mat-spinner /> }
  @case ('error')   { mensagem + "Tentar de novo" }
  @default          { @if (store.isEmpty()) { estado vazio } @else { lista } }
}
```
- `isEmpty` é um `computed` que só fica `true` **depois** de carregar. Assim a tela não pisca "Nenhuma conta" enquanto ainda está buscando.

### Recarregar depois de salvar
- Depois de criar, editar, arquivar ou excluir, a store chama `load()` de novo. Motivo: o **saldo total** é calculado no servidor (e no M3 vai depender de transações). Buscar de novo é mais simples e mais seguro do que recalcular no navegador.

### O diálogo que salva sozinho
- Quem abre o diálogo passa uma função `save`:
  ```ts
  this.dialog.open(AccountDialog, { data: { save: (req) => this.store.create(req) } });
  ```
- O diálogo chama `save`, **só fecha se der certo**, e se a API recusar (409 "nome já existe") mostra o erro **dentro** dele, sem perder o que foi digitado.
- O mesmo diálogo serve para criar e editar: com `account` nos dados, é edição.

### Componentes de apresentação: `input()` e `output()`
- `ColorPicker` e `IconPicker` não sabem nada de contas, categorias ou API. Recebem o valor atual (`input`) e avisam a escolha (`output`).
  ```html
  <app-color-picker [value]="form.controls.color.value" (selected)="form.controls.color.setValue($event)" />
  ```
- Por isso o mesmo seletor serve para contas e categorias.

### Dinheiro digitado em português
- O campo de saldo é **texto**, para aceitar "1.234,56". Só na hora de enviar, `parseMoney` converte para `1234.56`.
- "1.234" sem vírgula é **recusado**: é mil duzentos e trinta e quatro ou um vírgula dois três quatro? Na dúvida, o sistema pede para o usuário ser explícito.
- Na edição, `formatMoneyForInput(-320.1)` mostra "-320,10".

### `ng-template` + `ngTemplateOutlet`
- As abas Despesas e Receitas mostram a mesma lista, com dados diferentes. Um `<ng-template #list let-categories>` é escrito uma vez e "carimbado" nas duas abas.

### Responsividade com `BreakpointObserver` + `toSignal`
- O CDK avisa (Observable) quando a tela cruza 960 px; `toSignal` transforma isso em signal.
- Tela larga: menu lateral sempre aberto (`mode="side"`). Celular: botão ☰ e menu em gaveta (`mode="over"`), que fecha sozinho ao navegar.

### Confirmar antes de destruir
- Excluir abre um `ConfirmDialog` genérico. Com `afterClosed().pipe(filter(ok => ok), switchMap(() => store.delete(id)))`, a exclusão só acontece se o usuário confirmar.
- No 409 (a partir do M3, conta com transações), o aviso oferece **"Arquivar"** em vez de só mostrar o erro.

## 3. Testes (59 no frontend)

| Arquivo | O que garante |
|---------|---------------|
| `money.spec.ts` | 15 formatos aceitos e recusados ("1.234,56", "-320,10", "10,005"...) |
| `accounts.store.spec.ts` | Carregar, lista vazia só depois de carregar, recarregar após criar, mostrar arquivadas, erro |
| `account-dialog.spec.ts` | Conversão "1.234,56" → 1234.56, valor inválido, **409 dentro do diálogo sem fechar**, edição com saldo negativo |
| `accounts-page.spec.ts` | Cards com tipo em português, total, estado vazio, erro |
| `categories-page.spec.ts` | Abas com contagem por tipo, estado vazio por aba |

### Verificado no navegador (Docker + Nginx)
- Cadastro → menu lateral → **Categorias: Despesas (9) e Receitas (4)**.
- Contas vazia → "Nenhuma conta ainda" → criar "Nubank" com **1.234,56** → total **R$ 1.234,56**.
- Criar "NUBANK" → erro **dentro do diálogo**: "Você já tem uma conta chamada…".
- Cartão com -200,00 → total R$ 1.034,56; arquivar Nubank → total **-R$ 200,00 em vermelho**; "Mostrar arquivadas" a traz de volta marcada.
- Excluir pede confirmação.
- **Celular (375 px):** botão ☰, menu em gaveta que fecha ao navegar, sem rolagem horizontal.

---

## 🏋️ Exercícios sugeridos

1. **Siga uma conta do clique ao banco:** com F12 → Network aberto, crie uma conta. Veja o `POST /api/accounts` e, logo depois, o `GET /api/accounts`. Por que dois pedidos? (Releia "Recarregar depois de salvar".)
2. **Nova cor na paleta:** adicione uma cor em `COLOR_PALETTE` (`shared/appearance/appearance.ts`). Funciona sem mexer no backend? E se você adicionar um ícone novo em `ACCOUNT_ICONS`? (Dica: `AppearanceRules.cs`.)
3. **Formato de dinheiro:** escreva um caso de teste em `money.spec.ts` para "R$ -10,00". Ele passa? O que você esperaria?
4. **Ordenação:** faça a página de contas mostrar as contas da maior para a menor, por saldo, usando um `computed` na store. Escreva o teste antes.
5. **Desafio:** na página de categorias, adicione um campo de busca que filtra as categorias pelo nome enquanto você digita. Dica: um `signal` para o texto e um `computed` para a lista filtrada.
