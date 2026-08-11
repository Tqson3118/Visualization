# ── Stage 1: Build ────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Install build dependencies
RUN apk add --no-cache python3 make g++

# Install dependencies first for layer caching
COPY package.json package-lock.json ./
RUN npm ci --prefer-offline --no-audit --progress=false

# Copy source and build
COPY . .
ARG VITE_API_BASE_URL=http://localhost:5055
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ── Stage 2: Serve via nginx ─────────────────────────────
FROM nginx:alpine AS runtime

# Install curl for health checks
RUN apk add --no-cache curl

# Create non-root user
RUN addgroup -g 1001 -S nginx-user && \
    adduser -u 1001 -S -G nginx-user nginx-user

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Chỉnh main nginx.conf: pid vào /tmp để master chạy non-root được
RUN sed -i 's#pid        /run/nginx.pid;#pid        /tmp/nginx.pid;#' /etc/nginx/nginx.conf

# Copy custom nginx config and built assets
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

# Create directories for nginx cache and set permissions
RUN mkdir -p /var/cache/nginx /var/log/nginx /var/run && \
    chown -R nginx-user:nginx-user /var/cache/nginx /var/log/nginx /var/run /usr/share/nginx/html && \
    chmod 777 /tmp

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER nginx-user
CMD ["nginx", "-g", "daemon off;"]
