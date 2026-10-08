# 7. Observabilidade

[← TRD](README.md)

## Logs (Serilog)

- **Desenvolvimento:** console legível, nível `Debug` para o código do app e `Information` para o EF Core (mostra o SQL gerado, útil para aprender).
- **Produção:** console em **JSON** (`CompactJsonFormatter`), nível `Information`; `Warning` para `Microsoft.*` e EF Core. O provedor de hospedagem coleta a saída do container.
- `UseSerilogRequestLogging()`: uma linha por requisição com método, rota, status e duração.
- **Enriquecimento:** `TraceId`, `UserId` (quando autenticado), ambiente e versão da aplicação.

### O que nunca logar
Senhas, access tokens, refresh tokens, o header `Authorization`, cookies e o corpo de requisições de autenticação. Revisar isso em code review.

### Níveis
| Nível | Uso |
|-------|-----|
| `Information` | Eventos de negócio relevantes: usuário cadastrado, importação concluída, job executado |
| `Warning` | Situações anômalas e recuperáveis: reuso de refresh token detectado, lockout, rate limit |
| `Error` | Exceções não tratadas (`500`), falha de job |

## Correlation ID

- Usa o **trace id** do `Activity` do .NET (padrão W3C `traceparent`); não há middleware próprio.
- O mesmo id aparece no log da requisição, no campo `traceId` do ProblemDetails ([03-api.md](03-api.md#erros)) e no header de resposta `X-Trace-Id`.
- Quando o usuário relata um erro, o `traceId` mostrado na tela leva direto ao log.

## Health checks

| Endpoint | Verifica | Uso |
|----------|----------|-----|
| `GET /health/live` | O processo está de pé (sem dependências) | Liveness do container |
| `GET /health/ready` | Conexão com o PostgreSQL | Readiness, smoke test do deploy, badge de status |

Respostas em JSON com o status de cada verificação. Não exigem autenticação nem expõem detalhes internos (connection string, versões).

## Painel do Hangfire

- Disponível em `/hangfire` quando `Hangfire:DashboardEnabled=true`.
- Proteção: na demo, **somente leitura** (`IsReadOnlyFunc = true`) e acessível sem login, porque mostrar os jobs rodando faz parte da vitrine. Em qualquer outro ambiente, protegido por autenticação.
- Jobs registrados no MVP:

| Job | Agenda (America/Sao_Paulo) | Função |
|-----|----------------------------|--------|
| `reset-demo-user` | diário, 03:00 | Recria os dados do usuário demo ([modulos/06-usuario-demo.md](modulos/06-usuario-demo.md)) |
| `cleanup-refresh-tokens` | diário, 03:30 | Apaga tokens expirados ou revogados há mais de 30 dias |

## Fora de escopo (por enquanto)

Métricas (Prometheus/OpenTelemetry), tracing distribuído e alertas. Com um único serviço, logs estruturados + health checks bastam. OpenTelemetry é um bom candidato de evolução para o portfólio depois da Fase 3.
