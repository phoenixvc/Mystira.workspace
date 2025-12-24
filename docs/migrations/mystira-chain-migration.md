# Mystira.Chain Migration Guide

**Target**: Migrate Chain service to latest infrastructure standards
**Runtime**: Python 3.12+
**Estimated Effort**: 1 day
**Last Updated**: December 2025
**Status**: 📋 Planned

---

## Overview

Chain is a Python-based blockchain/ledger service using gRPC. Migration focuses on:

1. **Python 3.12 upgrade** (required)
2. **Dockerfile migration** to submodule repo (ADR-0019)
3. Infrastructure alignment with ADR-0017 (Resource Groups)
4. Kubernetes manifest standardization

---

## Current State Analysis

### Technology Stack

| Component | Current | Target |
|-----------|---------|--------|
| Python | 3.11 | 3.12+ |
| gRPC | grpcio | grpcio (latest) |
| Redis | redis-py | redis-py (latest) |
| Container Registry | Workspace builds | Submodule CI/CD |

### Infrastructure

| Resource | Location | Notes |
|----------|----------|-------|
| Key Vault | `mys-{env}-chain-rg-san` | Service-specific per ADR-0017 |
| Managed Identity | `mys-{env}-chain-rg-san` | Workload identity |
| App Insights | `mys-{env}-chain-rg-san` | Service telemetry |

---

## Phase 1: Python 3.12 Upgrade

### 1.1 Update pyproject.toml

```toml
[project]
requires-python = ">=3.12"

[tool.poetry]
python = "^3.12"
```

### 1.2 Update Dependencies

```bash
poetry update
```

### 1.3 Test Compatibility

```bash
poetry run pytest
poetry run mypy src/
```

---

## Phase 2: Dockerfile Migration (ADR-0019)

Move Dockerfile from workspace to submodule repo:

### 2.1 Create Dockerfile in Submodule

```dockerfile
# packages/chain/Dockerfile (new location)
FROM python:3.12-slim AS base

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Install poetry
RUN pip install poetry

# Copy dependency files
COPY pyproject.toml poetry.lock ./

# Install dependencies
RUN poetry config virtualenvs.create false \
    && poetry install --no-interaction --no-ansi --only main

# Copy application code
COPY src/ ./src/

EXPOSE 50051

CMD ["python", "-m", "mystira_chain.server"]
```

### 2.2 Add CI/CD Workflow

```yaml
# .github/workflows/ci.yml (in Mystira.Chain repo)
name: Chain CI

on:
  push:
    branches: [main, dev]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.12'
      - name: Install poetry
        run: pip install poetry
      - name: Install dependencies
        run: poetry install
      - name: Run tests
        run: poetry run pytest
      - name: Type check
        run: poetry run mypy src/

  docker:
    needs: test
    if: github.ref == 'refs/heads/dev'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          registry: myssharedacr.azurecr.io
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}
      - uses: docker/build-push-action@v5
        with:
          push: true
          tags: myssharedacr.azurecr.io/chain:${{ github.sha }}
      - name: Trigger workspace deployment
        uses: peter-evans/repository-dispatch@v2
        with:
          token: ${{ secrets.WORKSPACE_PAT }}
          repository: phoenixvc/Mystira.workspace
          event-type: chain-deploy
          client-payload: '{"sha": "${{ github.sha }}"}'
```

---

## Phase 3: Kubernetes Manifest Updates

### 3.1 Update Kustomization

Ensure manifests use `labels` instead of deprecated `commonLabels`:

```yaml
# kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

labels:
  - pairs:
      app.kubernetes.io/name: chain
      app.kubernetes.io/component: blockchain
    includeSelectors: true

resources:
  - deployment.yaml
  - service.yaml
  - configmap.yaml
```

### 3.2 Update Deployment

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mys-chain
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: chain
  template:
    metadata:
      labels:
        app.kubernetes.io/name: chain
    spec:
      serviceAccountName: mys-chain-sa
      containers:
        - name: chain
          image: myssharedacr.azurecr.io/chain:latest
          ports:
            - containerPort: 50051
              protocol: TCP
          env:
            - name: AZURE_KEY_VAULT_URL
              value: "https://mys-dev-chain-kv-san.vault.azure.net/"
```

---

## Phase 4: Observability Updates

### 4.1 OpenTelemetry Integration

```python
# src/mystira_chain/telemetry.py
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

def setup_telemetry():
    provider = TracerProvider()
    processor = BatchSpanProcessor(OTLPSpanExporter())
    provider.add_span_processor(processor)
    trace.set_tracer_provider(provider)
```

### 4.2 Structured Logging

```python
# src/mystira_chain/logging.py
import structlog

structlog.configure(
    processors=[
        structlog.stdlib.add_log_level,
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.processors.JSONRenderer(),
    ],
)

logger = structlog.get_logger()
```

---

## Migration Checklist

### Pre-Migration
- [ ] Backup environment variables
- [ ] Document current gRPC endpoints

### Phase 1: Python Upgrade
- [ ] Update pyproject.toml to Python 3.12
- [ ] Update dependencies
- [ ] Run tests and type checks

### Phase 2: Dockerfile Migration
- [ ] Create Dockerfile in submodule repo
- [ ] Add CI/CD workflow
- [ ] Test Docker build locally
- [ ] Remove Dockerfile from workspace

### Phase 3: Kubernetes
- [ ] Update kustomization.yaml with `labels`
- [ ] Update deployment.yaml
- [ ] Verify service account configuration

### Phase 4: Observability
- [ ] Add OpenTelemetry
- [ ] Add structured logging
- [ ] Verify traces in App Insights

### Post-Migration
- [ ] Test gRPC endpoints
- [ ] Verify Kubernetes deployment
- [ ] Monitor logs and traces
- [ ] Create PR

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| Python 3.11 → 3.12 | Runtime upgrade | Test thoroughly |
| Dockerfile location | CI/CD changes | Update workflows |
| Kustomize labels | K8s manifest syntax | Update all overlays |

---

## Related Documentation

- [ADR-0017: Resource Group Organization](../architecture/adr/0017-resource-group-organization-strategy.md)
- [ADR-0019: Dockerfile Location Standardization](../adr/ADR-0019-dockerfile-location-standardization.md)
- [Azure Resources Migration Summary](../../MIGRATION_SUMMARY.md)
- [Kubernetes Manifests](../../infra/k8s/chain/)
