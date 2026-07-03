# Multi-stage build for the Meridian frontend: Node builds the production
# bundle, nginx serves it and proxies /api to the api container. Build context
# is the client/ folder (see docker-compose.yml).
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/client/browser /usr/share/nginx/html
EXPOSE 80
