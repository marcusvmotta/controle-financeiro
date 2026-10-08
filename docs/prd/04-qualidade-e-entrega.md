# 4. Qualidade e entrega

[← Índice](README.md)

## Testes

- **Unitários (Fase 1):** regras de domínio (cálculo de saldo, transferência, validações) e services.
- **Integração (Fase 2):** endpoints reais com Postgres em container, incluindo o teste de **isolamento entre usuários**.
- **Frontend:** services, guards, interceptors e componentes principais (formulário de transação).
- Meta indicativa: ≥ 70% de cobertura nas camadas de regra de negócio. Cobertura é consequência, não objetivo.

## CI/CD (GitHub Actions)

- A cada push e PR: build do backend e do frontend, testes e lint.
- Na `main`: build das imagens Docker e deploy automático.
- Badges no README: build, testes e link da demo.

## Fluxo de trabalho no Git

- Branch por feature (`feat/transactions-crud`), PRs mesmo trabalhando sozinho, **Conventional Commits**.
- Issues e um GitHub Project (kanban) com as histórias deste PRD, referenciando os IDs `RF-xx`.

## README (vitrine do projeto)

- GIF ou screenshots do dashboard.
- Link da demo + credenciais do usuário demo.
- Diagrama da arquitetura.
- "Como rodar": `docker compose up` e mais nada.
- Seção **"Decisões técnicas"** (por que Postgres, por que camadas, por que `decimal`…), que é onde o avaliador vê o seu raciocínio. As decisões vêm do TRD.

## Definição de pronto (DoD) de cada módulo

- [ ] Critérios de aceite do módulo atendidos.
- [ ] Testes escritos e passando no CI.
- [ ] Endpoints documentados no Swagger.
- [ ] Tela responsiva e com estado vazio / de erro tratados.
- [ ] README atualizado, se o módulo mudar algo visível.
