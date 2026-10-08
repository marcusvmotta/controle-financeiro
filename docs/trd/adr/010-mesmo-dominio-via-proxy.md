# ADR-010 — Frontend e API no mesmo domínio via Nginx

**Status:** Aceito · **Data:** 2026-10-08

## Contexto

O refresh token vive num cookie `HttpOnly` com `SameSite=Strict` ([ADR-005](005-armazenamento-de-tokens.md)). Se frontend e API ficarem em domínios diferentes, o cookie vira "de terceiros": exige `SameSite=None`, CORS com credenciais, e alguns navegadores bloqueiam esse tipo de cookie por padrão.

## Decisão

Um container **`web` com Nginx** serve o build estático do Angular e faz **proxy reverso** de `/api/*` (e `/hangfire`, `/health`) para o container da API. Para o navegador, tudo vem da mesma origem.

- Localmente: `docker compose up` sobe `web`, `api` e `db`; o app fica em `http://localhost:8080`.
- Em produção: o endereço da API é configurado no Nginx por variável de ambiente (`API_UPSTREAM`).
- Em desenvolvimento com `ng serve`: o `proxy.conf.json` do Angular faz o mesmo papel do Nginx.

## Alternativas consideradas

- **Domínios separados (ex.: Vercel + Render)**: deploy do frontend mais simples, porém CORS com credenciais e cookie de terceiros.
- **A própria API servir o Angular** (`UseStaticFiles` + `MapFallbackToFile`): um único container, também sem CORS. Fica como **plano B** se o provedor de hospedagem tornar dois containers inviável.

## Consequências

- ✅ Sem CORS, cookie `SameSite=Strict` funciona, e o ambiente local é igual ao de produção.
- ✅ Nginx também cuida de cache de arquivos estáticos, gzip e do fallback de rotas da SPA (`try_files ... /index.html`).
- ⚠️ Um container a mais para manter e publicar.
- ⚠️ A API precisa confiar nos headers `X-Forwarded-*` do proxy (`UseForwardedHeaders`) para saber o IP e o esquema reais.
