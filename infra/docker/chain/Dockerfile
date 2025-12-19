# Mystira Chain Dockerfile
# Multi-stage build for Mystira blockchain/gRPC service (Python)

# Build stage
FROM python:3.11-slim AS builder

WORKDIR /app

# Install build dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Copy chain source code first
COPY packages/chain/ ./

# Create requirements.txt if it doesn't exist
RUN if [ ! -f requirements.txt ]; then \
    echo "grpcio>=1.50.0" > requirements.txt && \
    echo "grpcio-tools>=1.50.0" >> requirements.txt && \
    echo "protobuf>=4.21.0" >> requirements.txt; \
    fi

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Production stage
FROM python:3.11-slim AS production

WORKDIR /app

# Create non-root user
RUN groupadd -g 10000 chain && \
    useradd -u 10000 -g chain -s /bin/sh chain

# Install runtime dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    tini \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy from builder
COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /app ./

# Create data directory
RUN mkdir -p /data && chown -R chain:chain /data /app

# Set environment variables
ENV PYTHONUNBUFFERED=1
ENV CHAIN_DATA_DIR=/data

# Switch to non-root user
USER chain

# Expose ports (gRPC)
EXPOSE 50051

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:50051/health || exit 1

# Use tini as init
ENTRYPOINT ["/usr/bin/tini", "--"]

# Start the chain service
CMD ["python", "server.py"]
