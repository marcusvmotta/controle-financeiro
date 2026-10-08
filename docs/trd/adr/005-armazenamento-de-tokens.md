# ADR-005 — Access token em memória, refresh token em cookie HttpOnly

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T3

## Contexto

Uma SPA precisa guardar as credenciais da sessão em algum lugar. Tudo que o JavaScript consegue ler (`localStorage`, `sessionStorage`, variáveis) pode ser roubado por um ataque XSS. Cookies `HttpOnly` não são legíveis por JavaScript, mas são enviados automaticamente, o que abre espaço para CSRF.

## Decisão

- **Access token (JWT, 15 min):** devolvido no corpo da resposta e guardado **só em memória** (um signal no `AuthService`). Enviado no header `Authorization: Bearer`.
- **Refresh token (7 dias):** enviado num cookie `refresh_token` com `HttpOnly`, `Secure`, `SameSite=Strict` e `Path=/api/auth`. O JavaScript nunca o vê.
- Ao abrir ou recarregar o app, o frontend chama `POST /api/auth/refresh` para obter um novo access token. Se falhar, mostra o login.

## Alternativas consideradas

- **Os dois tokens no `localStorage`**: mais simples, mas um XSS rouba uma sessão de 7 dias.
- **Sessão só por cookie (sem JWT)**: válido, mas foge do objetivo de demonstrar JWT e exige proteção CSRF em todas as rotas.

## Consequências

- ✅ Um XSS só consegue usar o access token enquanto a página está aberta, e por no máximo 15 min; o refresh token não fica exposto.
- ✅ CSRF fica mitigado porque o cookie só vai para `/api/auth/*`, com `SameSite=Strict`, e frontend e API estão na mesma origem ([ADR-010](010-mesmo-dominio-via-proxy.md)). Como defesa extra, `/api/auth/refresh` e `/api/auth/logout` exigem o header `X-Requested-With: XMLHttpRequest`.
- ⚠️ Cada recarga da página faz uma chamada de refresh antes de mostrar a tela.
- ⚠️ Em desenvolvimento (`http://localhost`), `Secure` precisa ser relaxado ou o dev precisa usar HTTPS. Isso é controlado por configuração.
