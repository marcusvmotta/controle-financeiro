# M1b — Autenticação (frontend)

[← Diário](README.md) · **Backend:** [M1a](m1a-autenticacao-backend.md) · **Especificação:** [TRD 05-frontend](../trd/05-frontend.md), [TRD módulo 1](../trd/modulos/01-autenticacao.md)

**Objetivo do marco:** telas de login e cadastro, uma sessão que sobrevive a recarregar a página **sem guardar token em lugar nenhum acessível ao JavaScript**, e renovação automática do token.

---

## 1. Como as peças se encaixam

```mermaid
flowchart TD
    Init["provideAppInitializer<br/>tryRestoreSession()"] -->|cookie válido?| AS
    Pages["LoginPage / RegisterPage"] -->|login / register| AS["AuthService<br/>signals: user, accessToken"]
    AS --> Api["AuthApi<br/>(só HTTP)"]
    Guards["authGuard / guestGuard"] -->|isAuthenticated()| AS
    Any["Qualquer chamada /api"] --> Int["authInterceptor"]
    Int -->|Bearer token| AS
    Int -->|401 → refreshAccessToken()| AS
```

## 2. Conceitos

### Angular Material
- `ng add @angular/material` roda um **schematic**: instala o pacote **e** configura o projeto (tema no `styles.scss`, fontes).
- O tema é **Material 3**, definido por `mat.theme(...)`, que cria variáveis CSS como `--mat-sys-surface` e `--mat-sys-error`. Usamos essas variáveis nos nossos estilos, e assim o tema escuro sai quase de graça no futuro.
- Trocamos as fontes do Google Fonts por pacotes npm locais. Motivos: CSP restrita, privacidade e funcionar sem serviço externo.

### Signals no estado da sessão
```ts
private readonly _accessToken = signal<string | null>(null);   // só o service altera
readonly accessToken = this._accessToken.asReadonly();         // o resto do app só lê
readonly isAuthenticated = computed(() => this._accessToken() !== null);
```
- `computed` é um signal **derivado**: recalcula sozinho quando os signals que ele lê mudam.
- O padrão "privado gravável + público somente leitura" evita que um componente qualquer altere a sessão por engano.

### Por que o token some ao recarregar, e por que isso é bom
- O access token vive **só em memória**. Recarregar a página apaga tudo.
- Ao iniciar, `provideAppInitializer` chama `tryRestoreSession()`, que faz `POST /api/auth/refresh`. O **cookie HttpOnly** vai sozinho, e a API devolve um novo access token.
- Verificado no navegador: depois do reload, `document.cookie`, `localStorage` e `sessionStorage` estavam **vazios**, e mesmo assim o usuário continuava logado.

### Interceptors
- Funções que **envolvem** toda requisição do `HttpClient`, como uma "alfândega" na saída e na chegada.
- `authInterceptor`: coloca `Authorization: Bearer ...`; nas rotas `/api/auth/*` envia o cookie e o header anti-CSRF; no **401** renova a sessão e repete a requisição.
- `errorInterceptor`: mostra um aviso (*snackbar*) para servidor fora do ar e erros 5xx. Erros 4xx ficam com cada tela.
- **A ordem importa:** `[errorInterceptor, authInterceptor]`. O primeiro da lista é o mais "de fora".

### O refresh único (`shareReplay`)
O problema: o token expira, e a tela faz 3 chamadas ao mesmo tempo. As 3 recebem 401. Se cada uma pedisse um refresh, o backend veria o **mesmo cookie usado 3 vezes** e acharia que foi roubado (detecção de reuso do M1a), derrubando a sessão.

A solução:
```ts
this.refreshInFlight ??= this.api.refresh().pipe(
  ...,
  finalize(() => (this.refreshInFlight = null)),  // terminou: o próximo refresh será novo
  shareReplay({ bufferSize: 1, refCount: false }), // todos que pedirem agora recebem o MESMO resultado
);
```
- `??=` significa "só atribui se ainda for null". O primeiro que chama cria o refresh; os outros reaproveitam.
- `shareReplay` faz **uma** requisição HTTP e entrega o resultado para todos os inscritos.

### Guards
- `authGuard`: sem sessão → `/login?returnUrl=/onde-estava`.
- `guestGuard`: já logado → não deixa ver login e cadastro.
- ⚠️ **Guard não é segurança.** Ele só melhora a navegação. Quem protege os dados é a API, respondendo 401.

### Reactive Forms
- O formulário é um objeto no TypeScript (`NonNullableFormBuilder.group(...)`), com validadores declarados no código.
- **Validador de campo** (`letterAndDigit`) e **validador de grupo** (`passwordsMatch`, que compara dois campos).
- Os erros 400 da API são copiados para os campos com `applyServerValidationErrors`. As mensagens do backend aparecem embaixo do campo certo.

### Segurança no `returnUrl` (*open redirect*)
Um link malicioso como `/login?returnUrl=//site-falso.com` poderia mandar o usuário para outro site **depois** de ele digitar a senha no site verdadeiro. Por isso só aceitamos caminhos que começam com `/` e não com `//`.

### Lazy loading e budgets
- `loadComponent: () => import(...)` faz cada página virar um arquivo separado, baixado só quando aberto. Até o layout (`Shell`) só carrega depois do login.
- **Budget:** um alarme de tamanho no `angular.json`. O bundle inicial tem ~570 KB brutos, mas só ~130 KB trafegam (gzip).

## 3. Testes (29 no frontend)

| Arquivo | O que garante |
|---------|---------------|
| `auth.service.spec.ts` | Login guarda a sessão; **3 refreshes simultâneos = 1 requisição**; refresh com falha limpa a sessão; logout funciona mesmo com o servidor fora |
| `auth.interceptor.spec.ts` | Bearer nas chamadas; cookie + header nas rotas de auth; **teste obrigatório 10** (duas 401 simultâneas → um refresh → duas repetições); refresh falhou → login |
| `auth.guards.spec.ts` | Redirecionamentos com e sem sessão, incluindo o `returnUrl` |
| `login-page.spec.ts` | Validação, sucesso, `returnUrl`, **bloqueio de open redirect**, mensagem de credenciais inválidas |
| `register-page.spec.ts` | Senha com letra e número, confirmação, erro 400 no campo, conflito 409 |

Os testes de tela interagem pelo **HTML** (digitam nos inputs e enviam o form), como um usuário faria, em vez de mexer nas variáveis internas do componente.

### Verificado no navegador (Docker + Nginx)
- `/` sem login → `/login?returnUrl=%2Fdashboard`.
- Cadastro com senha fraca e confirmação diferente → erros na tela, nada enviado.
- Cadastro válido → dashboard com "Olá, Marcus Teste!".
- **Reload** → continua logado; `document.cookie`, `localStorage` e `sessionStorage` vazios.
- Logout → `/login`; reload em `/dashboard` → login de novo (token revogado no servidor).
- Senha errada → "E-mail ou senha inválidos."; senha certa → volta ao `returnUrl`.
- ⚠️ Não testado no navegador: o token expirar com a página aberta (levaria 15 min). Está coberto pelos testes do interceptor.

---

## 🏋️ Exercícios sugeridos

1. **Veja o refresh acontecendo:** abra o app logado, aperte F12 → aba **Network**, recarregue a página e encontre a chamada `refresh`. Olhe os **Request Headers**: o cookie `refresh_token` está lá. Agora rode `document.cookie` no **Console** e confirme que ele não aparece.
2. **Veja o cookie de verdade:** F12 → **Application** → **Cookies** → `localhost`. Repare nas colunas `HttpOnly`, `Secure`, `SameSite` e `Path`.
3. **Expire o token na marra:** em `backend/src/FinanceControl.Api/appsettings.Development.json`, coloque `"AccessTokenMinutes": 1` dentro de `Jwt`. Rode a API com `dotnet watch` e o frontend com `npm start`, faça login, espere 2 minutos e clique em "Verificar novamente". Na aba Network você verá: chamada → 401 → refresh → chamada repetida com sucesso. **Desfaça a mudança depois.**
4. **Quebre o refresh único:** comente a linha do `shareReplay` no `auth.service.ts` e rode `npx ng test --watch=false`. Quais testes quebram e por quê? Volte a linha.
5. **Novo validador:** na tela de cadastro, impeça que a senha contenha o próprio e-mail. Crie um validador de grupo e um teste para ele.
