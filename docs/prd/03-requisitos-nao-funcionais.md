# 3. Requisitos não-funcionais

[← Índice](README.md)

Valem para **todos** os módulos.

| ID | Área | Requisito |
|----|------|-----------|
| RNF-01 | **Segurança** | HTTPS; senhas com hash; JWT com expiração curta; refresh token rotacionado; CORS restrito ao domínio do frontend; validação de entrada em todas as rotas; nenhum segredo no repositório (variáveis de ambiente / `dotnet user-secrets`). |
| RNF-02 | **Isolamento** | Toda consulta filtrada pelo usuário logado, coberta por teste automatizado. |
| RNF-03 | **Dinheiro** | Valores como `decimal(18,2)` no C# e `numeric(18,2)` no Postgres. **Nunca** `float`/`double`. Moeda única: BRL. |
| RNF-04 | **Datas** | Armazenar em UTC; exibir no fuso do usuário (padrão `America/Sao_Paulo`). A data da transação é uma data de competência, sem hora. |
| RNF-05 | **Performance** | Listagens paginadas; lista de transações em < 500 ms com 10 mil registros. |
| RNF-06 | **Erros** | Respostas de erro no padrão **ProblemDetails** (RFC 9457); erros de validação indicam campo e mensagem. |
| RNF-07 | **Observabilidade** | Logs estruturados (JSON em produção) com correlation ID por requisição; endpoint `/health` verificando também o banco. |
| RNF-08 | **Documentação da API** | **Swagger/OpenAPI** com autenticação JWT configurada no Swagger UI. |
| RNF-09 | **Acessibilidade** | Navegação por teclado, labels em formulários, contraste adequado. |
| RNF-10 | **Responsividade** | Utilizável em telas a partir de 360 px de largura. |
| RNF-11 | **Idioma e formato** | Interface em PT-BR; moeda `R$ 1.234,56`; datas `dd/MM/yyyy`. Código, nomes de tabelas e endpoints em inglês. |
| RNF-12 | **Portabilidade** | Toda a aplicação sobe com `docker compose up`, sem dependências instaladas na máquina além do Docker. |
