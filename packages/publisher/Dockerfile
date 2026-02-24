# Mystira Publisher Dockerfile
# Multi-stage build for Mystira Publisher SPA

# Build stage
FROM node:20-alpine@sha256:09e2b3d9726018aecf269bd35325f46bf75046a643a66d28360ec71132750ec8 AS builder

WORKDIR /app

# Install pnpm via corepack
RUN corepack enable && corepack prepare pnpm@latest --activate

# Copy package files
COPY package.json pnpm-lock.yaml ./

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy source files
COPY . .

# Build the application
RUN pnpm build

# Production stage
FROM nginx:alpine@sha256:1d13701a5f9f3fb01aaa88cef2344d65b6b5bf6b7d9fa4cf0dca557a8d7702ba AS production

# Install tini for proper signal handling
RUN apk add --no-cache tini curl

# Create non-root user
RUN addgroup -g 10000 publisher && \
    adduser -u 10000 -G publisher -s /bin/sh -D publisher

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Set ownership
RUN chown -R publisher:publisher /usr/share/nginx/html && \
    chown -R publisher:publisher /var/cache/nginx && \
    chown -R publisher:publisher /var/log/nginx && \
    touch /var/run/nginx.pid && \
    chown -R publisher:publisher /var/run/nginx.pid

# Set environment variables
ENV NODE_ENV=production

# Switch to non-root user
USER publisher

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Use tini as init for proper signal handling
ENTRYPOINT ["/sbin/tini", "--"]

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
