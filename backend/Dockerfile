# ---- Estágio 1: build (imagem com o SDK completo) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia primeiro só os arquivos de projeto e restaura os pacotes.
# Como essa camada só muda quando um .csproj muda, o Docker reaproveita o cache
# e não baixa os pacotes NuGet de novo a cada alteração de código.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/FinanceControl.Domain/FinanceControl.Domain.csproj src/FinanceControl.Domain/
COPY src/FinanceControl.Application/FinanceControl.Application.csproj src/FinanceControl.Application/
COPY src/FinanceControl.Infrastructure/FinanceControl.Infrastructure.csproj src/FinanceControl.Infrastructure/
COPY src/FinanceControl.Api/FinanceControl.Api.csproj src/FinanceControl.Api/
RUN dotnet restore src/FinanceControl.Api/FinanceControl.Api.csproj

# Agora o código-fonte, e a publicação em modo Release.
COPY src/ src/
RUN dotnet publish src/FinanceControl.Api/FinanceControl.Api.csproj \
    --configuration Release --no-restore --output /app/publish

# ---- Estágio 2: runtime (imagem mínima, só para executar) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Roda como usuário sem privilégios de root (o usuário "app" já existe na imagem oficial).
USER $APP_UID

# A imagem oficial do ASP.NET escuta na porta 8080 por padrão.
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinanceControl.Api.dll"]
