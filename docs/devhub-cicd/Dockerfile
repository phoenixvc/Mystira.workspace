# Dockerfile for Leptos SSR application
# Multi-stage build for minimal final image

# ============================================
# Stage 1: Build the application
# ============================================
FROM rust:1.92-slim-bookworm@sha256:a8ce22642819f5f54d37e025f7020b51dcbac39e5d0ea907a63eda8b03458de5 AS builder

# Install build dependencies
RUN apt-get update && apt-get install -y \
    pkg-config \
    libssl-dev \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install wasm32 target and trunk
RUN rustup target add wasm32-unknown-unknown
RUN cargo install trunk --locked

WORKDIR /app

# Cache dependencies by copying only Cargo files first
COPY Cargo.toml Cargo.lock ./
RUN mkdir src && echo "fn main() {}" > src/main.rs
RUN cargo build --release || true
RUN rm -rf src

# Copy source and build
COPY . .

# Build with trunk for WASM + server
ARG LEPTOS_ENV=production
ENV LEPTOS_ENV=${LEPTOS_ENV}

# For SSR apps, build the server binary
RUN cargo build --release --features ssr

# Build frontend assets with trunk
RUN trunk build --release

# ============================================
# Stage 2: Runtime image
# ============================================
FROM debian:bookworm-slim@sha256:d5d3f9c23164ea16f31852f95bd5959aad1c5e854332fe00f7b3a20fcc9f635c AS runtime

# Install runtime dependencies
RUN apt-get update && apt-get install -y \
    ca-certificates \
    libssl3 \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN useradd -m -u 1000 -s /bin/bash app
USER app

WORKDIR /app

# Copy built artifacts
COPY --from=builder --chown=app:app /app/target/release/devhub /app/devhub
COPY --from=builder --chown=app:app /app/dist /app/dist

# Environment
ENV RUST_LOG=info
ENV LEPTOS_SITE_ADDR=0.0.0.0:3000
ENV LEPTOS_SITE_ROOT=dist
ENV LEPTOS_ENV=production

EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:3000/health || exit 1

CMD ["./devhub"]
