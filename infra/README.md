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
- Azure CLI (for Azure deployments)

### Azure Setup

For Azure deployments, you need to configure a service principal with appropriate permissions. See [AZURE_SETUP.md](./AZURE_SETUP.md) for detailed instructions on:

- Creating and configuring a service principal
- Required permissions (Contributor role at subscription level)
- Setting up GitHub Actions secrets
- Troubleshooting authorization issues

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
- **DNS**: Azure DNS zones for domain management

### DNS Configuration

The infrastructure now includes DNS zones and records for:

- `publisher.mystira.app` - Publisher service endpoint
- `chain.mystira.app` - Blockchain RPC/WebSocket endpoint

After deploying the Terraform infrastructure, you need to:

1. Get the name servers from the Terraform output:

   ```bash
   cd terraform/environments/prod
   terraform output dns_name_servers
   ```

2. Configure these name servers in your domain registrar for `mystira.app`

3. The DNS module will automatically create A records pointing to the ingress load balancer IPs

### Kubernetes Deployments

The Kubernetes manifests include:

- **Deployments/StatefulSets**: Chain nodes, Publisher service
- **Services**: Internal and external endpoints
- **Ingress**: HTTPS ingress for public endpoints with SSL/TLS
- **Cert-Manager**: Automated SSL certificate management via Let's Encrypt
- **ConfigMaps/Secrets**: Configuration and credentials

```bash
# Deploy to dev
kubectl apply -k kubernetes/overlays/dev

# Deploy to staging
kubectl apply -k kubernetes/overlays/staging

# Deploy to production
kubectl apply -k kubernetes/overlays/prod
```

### SSL/TLS Certificates

The infrastructure uses cert-manager with Let's Encrypt for automatic SSL certificate provisioning:

- Certificates are automatically requested for `publisher.mystira.app` and `chain.mystira.app`
- Certificates auto-renew before expiration
- Uses HTTP-01 challenge for validation

To install cert-manager in your cluster (if not already installed):

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

### Docker Images

```bash
# Build all images
./scripts/build-images.sh

# Push to registry
./scripts/push-images.sh
```

## Environments

| Environment | Purpose           | URL                |
| ----------- | ----------------- | ------------------ |
| Development | Local/Dev testing | dev.mystira.io     |
| Staging     | Pre-production    | staging.mystira.io |
| Production  | Live platform     | mystira.io         |

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
