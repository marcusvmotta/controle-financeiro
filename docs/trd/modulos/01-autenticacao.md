# Módulo 1 — Autenticação (técnico)

[← TRD](../README.md) · **PRD:** [RF-01](../../prd/modulos/01-autenticacao.md) · **Decisões:** [ADR-004](../adr/004-identity-e-jwt.md), [ADR-005](../adr/005-armazenamento-de-tokens.md) · **Segurança:** [04-seguranca.md](../04-seguranca.md)

## Componentes

| Camada | Componente | Responsabilidade |
|--------|------------|------------------|
| Infrastructure | `AppUser : IdentityUser<Guid>` | Usuário com `Name`, `IsDemo`, `CreatedAt` |
| Infrastructure | `JwtTokenGenerator` | Gera o JWT e o refresh token (aleatório) e calcula o hash do refresh token |
| Application | `IAuthService`, DTOs, validators | Contrato dos casos de uso e regras de entrada |
| Infrastructure | `AuthService : IAuthService` | Cadastro, login, refresh (rotação e detecção de reuso), logout. Fica na Infrastructure porque depende do `UserManager` do Identity |
| Api | `AuthController`, `MeController` | Endpoints; escreve e apaga o cookie |
| Api | `CurrentUser : ICurrentUser` | Lê `sub` do `HttpContext.User` |

## Endpoints

### `POST /api/auth/register`
```json
// request
{ "name": "Ana Souza", "email": "ana@email.com", "password": "senha1234" }
```
- `201` → mesmo corpo do login + cookie (o usuário já entra logado).
- `400` validação · `409` e-mail já cadastrado.
- Na mesma transação de banco: cria o usuário e copia as categorias padrão. *(A cópia das categorias entra no M2, junto com o módulo de categorias.)*

### `POST /api/auth/login`
```json
// request
{ "email": "ana@email.com", "password": "senha1234" }

// 200 response (+ Set-Cookie: refresh_token=...)
{
  "accessToken": "eyJhbGciOi...",
  "expiresAt": "2026-10-08T14:45:00Z",
  "user": { "id": "0192...", "name": "Ana Souza", "email": "ana@email.com", "isDemo": false }
}
```
- `401` credenciais inválidas, com mensagem genérica. Durante o lockout também responde `401` com a mesma mensagem, para não revelar que a conta existe.
- `429` rate limit.

### `POST /api/auth/refresh`
- Sem corpo. Lê o cookie `refresh_token`; exige o header `X-Requested-With`.
- `200` → mesmo corpo do login + novo cookie.
- `401` → cookie ausente, expirado, revogado ou reusado (neste caso revoga a família). A resposta também apaga o cookie.

### `POST /api/auth/logout`
- Revoga o refresh token do cookie e apaga o cookie. `204` sempre (mesmo sem cookie válido).

### `POST /api/auth/demo`
- *Implementado no M5, com o módulo do usuário demo.*
- Só registrado quando `Demo:Enabled=true`. Faz login no usuário demo sem senha. Ver [06-usuario-demo.md](06-usuario-demo.md).

### `GET /api/me`
- `200` → `{ id, name, email, isDemo }`.

## Validações (FluentValidation)

| Campo | Regras |
|-------|--------|
| `name` | obrigatório, 2–100 caracteres |
| `email` | obrigatório, formato válido, até 254 caracteres; normalizado para minúsculas |
| `password` | 8–128 caracteres, pelo menos uma letra e um número |

## Frontend

- `AuthService`: signals `user` e `accessToken`; métodos `login`, `register`, `logout`, `tryRefresh`, `loginAsDemo`.
- `authInterceptor`, `authGuard`, `guestGuard` (ver [05-frontend.md](../05-frontend.md#autenticação-no-frontend)).
- Páginas `/login` (com botão "Entrar como demo" quando habilitado) e `/register`.
- O frontend sabe se o modo demo está ativo por `GET /api/auth/config` → `{ demoEnabled: boolean }`.

## Testes específicos

Testes obrigatórios 1–4 e 10 de [08-testes.md](../08-testes.md#testes-obrigatórios-não-negociáveis), mais:
- cadastro cria as categorias padrão para o novo usuário;
- lockout após 5 tentativas;
- e-mail com maiúsculas é tratado como o mesmo e-mail.
