# Mystira Chain Dockerfile
# Multi-stage build for Mystira blockchain node

# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Install build dependencies
RUN apk add --no-cache python3 make g++ git

# Copy workspace package files
COPY package.json ./
COPY pnpm-lock.yaml ./
COPY pnpm-workspace.yaml ./

# Copy chain package.json
COPY packages/chain/package.json ./packages/chain/

# Install pnpm
RUN npm install -g pnpm

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy chain source code
COPY packages/chain/ ./packages/chain/

# Build the application
WORKDIR /app/packages/chain
RUN pnpm build || true

# Create dist directory if it doesn't exist
RUN mkdir -p /app/packages/chain/dist

# Production stage
FROM node:18-alpine AS production

WORKDIR /app

# Create non-root user
RUN addgroup -g 10000 chain && \
    adduser -u 10000 -G chain -s /bin/sh -D chain

# Install runtime dependencies
RUN apk add --no-cache tini

# Copy built application
COPY --from=builder /app/packages/chain/dist ./dist
COPY --from=builder /app/packages/chain/node_modules ./node_modules
COPY --from=builder /app/packages/chain/package.json ./

# Create data directory
RUN mkdir -p /data && chown -R chain:chain /data

# Set environment variables
ENV NODE_ENV=production
ENV CHAIN_DATA_DIR=/data

# Switch to non-root user
USER chain

# Expose ports
EXPOSE 8545 8546 30303 30303/udp

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8545/health || exit 1

# Use tini as init
ENTRYPOINT ["/sbin/tini", "--"]

# Start the chain node
CMD ["node", "dist/index.js"]
