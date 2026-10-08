# ADR-004 — ASP.NET Core Identity + JWT próprio

**Status:** Aceito · **Data:** 2026-10-08 · **Resolve:** T2

## Contexto

O app precisa de cadastro, login, hash de senha, regras de senha e proteção contra força bruta. A autenticação da API será via JWT Bearer (o frontend é uma SPA). Implementar hash e bloqueio na mão é fácil de errar.

## Decisão

Usar **ASP.NET Core Identity** apenas no "núcleo" (`AddIdentityCore<AppUser>()` + `AddEntityFrameworkStores`) para gerenciar usuários, hash de senha, regras de senha e bloqueio por tentativas. A emissão do **access token JWT** e do **refresh token** é feita por um serviço próprio (`ITokenService`).

Não usar os endpoints prontos do Identity (`MapIdentityApi`), porque eles emitem um token próprio, não JWT, e dão menos controle sobre o fluxo de refresh e cookies.

## Alternativas consideradas

- **Tabela própria + `PasswordHasher<T>`**: mais simples de entender, mas reimplementa regras de senha, lockout e normalização de e-mail.
- **`MapIdentityApi`**: pouco código, mas usa token opaco do Identity e não permite o desenho de cookie do [ADR-005](005-armazenamento-de-tokens.md).
- **Provedor externo (Auth0, Keycloak, Entra ID)**: robusto, mas esconde justamente o que o projeto quer demonstrar e adiciona dependência externa.

## Consequências

- ✅ Hash de senha (PBKDF2), lockout e validação de senha prontos e testados.
- ✅ O fluxo de tokens fica explícito no código, o que é bom para estudo e para o portfólio.
- ⚠️ As tabelas do Identity (`AspNetUsers` etc.) são renomeadas para `snake_case` no `OnModelCreating`, e as que não são usadas (roles, claims, logins externos) não são criadas.
- ⚠️ A responsabilidade por rotação e revogação de refresh tokens é nossa (detalhada em [04-seguranca.md](../04-seguranca.md)).
