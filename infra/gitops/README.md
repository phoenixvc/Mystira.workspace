# Harness GitOps Agent Setup

This directory contains the Harness GitOps agent configuration and GitOps application definitions.

## Structure

```
infra/gitops/
├── harness-agent.yaml      # Agent configuration (one-time setup)
├── README.md               # This file
├── applications/           # GitOps Application definitions
│   ├── admin-api.yaml
│   ├── publisher.yaml
│   └── story-generator.yaml
└── bootstrap-gitops.sh     # One-time setup script
```

## Why Not in CI/CD?

**The agent is the deployer, not the deployed.**

- The Harness GitOps agent watches Git and deploys changes automatically
- Running agent installation in CI/CD would:
  - Re-install the agent on every pipeline run (unnecessary)
  - Risk exposing agent tokens in CI/CD logs
  - Create race conditions with ongoing syncs

## One-Time Setup

### Prerequisites

- `kubectl` configured with access to your Kubernetes cluster
- Harness GitOps agent token (from Harness platform)
- Agent YAML file from Harness platform

### Installation

1. **Store the agent YAML** in `harness-agent.yaml` (redact secrets - see Security Note below)

2. **Run the bootstrap script**:

   ```bash
   ./infra/scripts/bootstrap-gitops.sh
   ```

   Or manually:

   ```bash
   kubectl create namespace harness-gitops --dry-run=client -o yaml | kubectl apply -f -
   envsubst < infra/gitops/harness-agent.yaml | kubectl apply -f - -n harness-gitops
   kubectl wait --for=condition=ready pod -l app=harness-gitops-agent -n harness-gitops --timeout=120s
   ```

3. **Verify installation**:
   ```bash
   kubectl get pods -n harness-gitops
   kubectl logs -l app=harness-gitops-agent -n harness-gitops
   ```

## Security Note

**Never commit secrets to Git!**

The `harness-agent.yaml` file should use environment variable substitution for sensitive values:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: harness-gitops-agent
stringData:
  HARNESS_AGENT_TOKEN: "${HARNESS_AGENT_TOKEN}" # Injected at apply time
```

Apply with:

```bash
export HARNESS_AGENT_TOKEN="your-token-here"
envsubst < infra/gitops/harness-agent.yaml | kubectl apply -f -
```

Alternatively, use a secrets management solution (Azure Key Vault, External Secrets Operator, etc.).

## GitOps Applications

Once the agent is connected, define your applications in `applications/`:

- `admin-api.yaml` - Admin API deployment
- `publisher.yaml` - Publisher service deployment
- `story-generator.yaml` - Story Generator service deployment

These applications will be automatically synced by the Harness GitOps agent when changes are pushed to Git.

## CI/CD Integration

**What TO put in CI/CD:**

- Validation of Kubernetes manifests
- Triggering Harness sync via API (optional)
- Testing application configurations

See `.github/workflows/gitops-apps.yml` for an example workflow that validates manifests and optionally triggers syncs.

**Note:** The workflow validates manifests but does NOT install or update the agent. Agent installation is a one-time manual operation.

**What NOT to put in CI/CD:**

- Agent installation/updates
- Direct `kubectl apply` of agent YAML
- Agent token management

## Troubleshooting

### Agent not connecting

1. Check agent pod logs:

   ```bash
   kubectl logs -l app=harness-gitops-agent -n harness-gitops
   ```

2. Verify token is correct:

   ```bash
   kubectl get secret gitops-agent -n harness-gitops -o jsonpath='{.data.GITOPS_AGENT_TOKEN}' | base64 -d
   ```

3. Check network policies allow agent communication

### Applications not syncing

1. Verify agent is watching the correct Git repository
2. Check application definitions in `applications/`
3. Review Harness platform for sync status
4. Check application controller logs:
   ```bash
   kubectl logs -l app.kubernetes.io/name=argocd-application-controller -n harness-gitops
   ```

## References

- [Harness GitOps Documentation](https://docs.harness.io/category/gitops)
- [Argo CD Documentation](https://argo-cd.readthedocs.io/)
