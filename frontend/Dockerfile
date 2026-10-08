# ---- Estágio 1: build do Angular (imagem com Node) ----
FROM node:22-alpine AS build
WORKDIR /app

# Mesma ideia do backend: dependências primeiro, para aproveitar o cache do Docker.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npx ng build --configuration production

# ---- Estágio 2: Nginx servindo os arquivos estáticos ----
FROM nginx:stable-alpine AS runtime

# Arquivos em /etc/nginx/templates passam pelo envsubst ao iniciar o container:
# ${API_UPSTREAM} é trocado pelo valor da variável de ambiente.
COPY nginx/security-headers.conf /etc/nginx/snippets/security-headers.conf
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80
