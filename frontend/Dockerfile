# ── Frontend Dockerfile — multi-stage (pattern tham khảo VisualizationDSA V1, viết lại: Node 22 + nginx non-root, proxy /api → backend:8080) ──

# ── Stage 1: Build ────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies first for layer caching
COPY package.json package-lock.json ./
RUN npm ci --prefer-offline --no-audit --progress=false

# Copy source and build
COPY . .
ARG VITE_API_BASE_URL=/api/v1
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ── Stage 2: Serve via nginx ─────────────────────────────
FROM nginx:alpine AS runtime

# curl cho health check
RUN apk add --no-cache curl

# Non-root user
RUN addgroup -g 1001 -S nginx-user && \
    adduser -u 1001 -S -G nginx-user nginx-user

# Remove default config, dùng config riêng
RUN rm /etc/nginx/conf.d/default.conf

# pid vào /tmp để master chạy non-root
RUN sed -i 's#pid        /run/nginx.pid;#pid        /tmp/nginx.pid;#' /etc/nginx/nginx.conf

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

RUN mkdir -p /var/cache/nginx /var/log/nginx /var/run && \
    chown -R nginx-user:nginx-user /var/cache/nginx /var/log/nginx /var/run /usr/share/nginx/html && \
    chmod 777 /tmp

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER nginx-user
CMD ["nginx", "-g", "daemon off;"]
