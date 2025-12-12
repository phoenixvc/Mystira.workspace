# Mystira Publisher Dockerfile
# Multi-stage build for Mystira Publisher service

# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Install build dependencies
RUN apk add --no-cache python3 make g++

# Copy package files
COPY package*.json ./
COPY pnpm-lock.yaml ./

# Install pnpm
RUN npm install -g pnpm

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy source code
COPY . .

# Build the application
RUN pnpm build

# Prune dev dependencies
RUN pnpm prune --prod

# Production stage
FROM node:18-alpine AS production

WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 publisher && \
    adduser -u 1000 -G publisher -s /bin/sh -D publisher

# Install runtime dependencies
RUN apk add --no-cache tini curl

# Copy built application
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./

# Set ownership
RUN chown -R publisher:publisher /app

# Set environment variables
ENV NODE_ENV=production
ENV PORT=3000
ENV HEALTH_PORT=3001

# Switch to non-root user
USER publisher

# Expose ports
EXPOSE 3000 3001

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:3001/health || exit 1

# Use tini as init
ENTRYPOINT ["/sbin/tini", "--"]

# Start the publisher service
CMD ["node", "dist/index.js"]
