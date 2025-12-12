# Mystira.Infra

Infrastructure, DevOps, and deployment configurations for the Mystira platform.

## Overview

Mystira.Infra manages all infrastructure concerns including:

- Cloud infrastructure provisioning
- Kubernetes deployments
- CI/CD pipelines
- Monitoring and observability
- Security configurations

## Structure

```
infra/
├── terraform/         # Infrastructure as Code
│   ├── modules/      # Reusable Terraform modules
│   ├── environments/ # Environment-specific configs
│   │   ├── dev/
│   │   ├── staging/
│   │   └── prod/
│   └── shared/       # Shared resources
├── kubernetes/        # K8s manifests
│   ├── base/         # Base configurations
│   ├── overlays/     # Environment overlays
│   └── charts/       # Helm charts
├── docker/            # Dockerfiles
│   ├── app/
│   ├── api/
│   └── worker/
└── scripts/           # DevOps automation scripts
    ├── deploy/
    ├── backup/
    └── monitoring/
```

## Getting Started

### Prerequisites

- Terraform >= 1.5
- kubectl
- Helm >= 3.0
- Docker
- AWS CLI / GCP CLI

### Initial Setup

```bash
# Initialize Terraform
cd terraform/environments/dev
terraform init

# Plan infrastructure changes
terraform plan

# Apply changes
terraform apply
```

## Infrastructure Components

### Cloud Resources (Terraform)

- **Compute**: Kubernetes clusters, VM instances
- **Networking**: VPCs, load balancers, CDN
- **Storage**: Object storage, databases
- **Security**: IAM, secrets management

### Kubernetes Deployments

```bash
# Deploy to dev
kubectl apply -k kubernetes/overlays/dev

# Deploy to staging
kubectl apply -k kubernetes/overlays/staging

# Deploy to production
kubectl apply -k kubernetes/overlays/prod
```

### Docker Images

```bash
# Build all images
./scripts/build-images.sh

# Push to registry
./scripts/push-images.sh
```

## Environments

| Environment | Purpose | URL |
|------------|---------|-----|
| Development | Local/Dev testing | dev.mystira.io |
| Staging | Pre-production | staging.mystira.io |
| Production | Live platform | mystira.io |

## CI/CD Pipeline

```
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│   Code   │───▶│   Build  │───▶│   Test   │───▶│  Deploy  │
│   Push   │    │          │    │          │    │          │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
                     │                               │
                     ▼                               ▼
               ┌──────────┐                   ┌──────────┐
               │  Lint &  │                   │ Release  │
               │  Scan    │                   │  Notes   │
               └──────────┘                   └──────────┘
```

## Monitoring

### Observability Stack

- **Metrics**: Prometheus + Grafana
- **Logging**: Loki / ELK Stack
- **Tracing**: Jaeger / OpenTelemetry
- **Alerting**: PagerDuty / Slack

### Dashboards

Access dashboards at: `monitoring.mystira.io/grafana`

## Security

- Secrets managed via HashiCorp Vault
- Network policies enforced in K8s
- Regular security scanning
- SSL/TLS everywhere

## Disaster Recovery

- Automated backups (daily)
- Multi-region deployment
- Documented runbooks
- Regular DR drills

## Scripts

```bash
# Database backup
./scripts/backup/db-backup.sh

# Scale deployment
./scripts/deploy/scale.sh app 5

# Rollback deployment
./scripts/deploy/rollback.sh app v1.2.3
```

## Environment Variables

Infrastructure secrets are managed through environment-specific `.tfvars` files and Kubernetes secrets.

```bash
# Example terraform.tfvars
project_id = "mystira-prod"
region     = "us-central1"
env        = "production"
```

## License

Proprietary - All rights reserved

