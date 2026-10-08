# Módulo 1 — Autenticação

[← Índice](../README.md) · **Fase:** MVP · **Requisito:** RF-01 · **Prioridade:** P0

## Objetivo

Permitir que cada pessoa tenha sua conta no sistema, com dados totalmente isolados dos demais usuários.

## Histórias de usuário

- Como **visitante**, quero me cadastrar com nome, e-mail e senha para começar a usar o app.
- Como **usuário**, quero fazer login e continuar logado enquanto uso o app, sem ser deslogado a cada poucos minutos.
- Como **usuário**, quero sair da minha conta para que ninguém mais acesse meus dados naquele dispositivo.
- Como **usuário**, quero ter certeza de que nenhuma outra pessoa vê as minhas finanças.

## Requisitos

- Cadastro com nome, e-mail e senha.
- Login retorna **access token JWT** (curta duração, ~15 min) + **refresh token** (longa duração, armazenado no banco, rotacionado a cada uso).
- Logout invalida o refresh token.
- Senha armazenada somente como hash.
- Rota para consultar os dados do usuário logado.

## Critérios de aceite

- [ ] E-mail duplicado retorna `409 Conflict`.
- [ ] Senha fraca (< 8 caracteres, sem número ou letra) é rejeitada com mensagem clara.
- [ ] Rotas protegidas retornam `401` sem token válido.
- [ ] O frontend renova o access token automaticamente sem deslogar o usuário.
- [ ] Um usuário **nunca** acessa dados de outro (teste automatizado garante isso).

## Fora de escopo

- Login social (Google, GitHub).
- Recuperação de senha por e-mail *(candidato a P1 depois do MVP)*.
- Confirmação de e-mail e 2FA.

## Insumos para o TRD

- **Entidades:** `User (Id, Name, Email, PasswordHash, CreatedAt)`, `RefreshToken (Id, UserId, TokenHash, ExpiresAt, RevokedAt, ReplacedByTokenId)`.
- **Endpoints preliminares:** `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout`, `GET /api/me`.
- **Decidir no TRD:**
  - ASP.NET Core Identity completo ou só `PasswordHasher` + tabela própria?
  - Onde o frontend guarda os tokens (memória + cookie `HttpOnly` para o refresh, ou `localStorage`)? Impacta segurança contra XSS.
  - Política de expiração do refresh token e detecção de reuso de token roubado.
  - Como garantir o isolamento: EF Core *global query filter* por `UserId`.
- **Frontend:** auth service, HTTP interceptor (anexa token e renova em `401`), route guard.
