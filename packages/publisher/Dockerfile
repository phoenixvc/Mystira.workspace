# Mystira Publisher Dockerfile
# Multi-stage build for Mystira Publisher SPA

# Build stage
FROM node:20-alpine AS builder

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
FROM nginx:alpine AS production

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
