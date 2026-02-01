# Istio Service Mesh Configuration

This directory contains Istio service mesh configuration for enabling mTLS (mutual TLS) across all Mystira services.

## Overview

The service mesh configuration provides:
- **Mutual TLS (mTLS)**: Automatic encryption of all service-to-service traffic
- **Identity-based access control**: Services authenticate using cryptographic identities
- **Zero-trust networking**: Default deny policies with explicit allow rules
- **Traffic management**: Connection pooling, circuit breaking, and outlier detection

## Prerequisites

### 1. Install Istio

```bash
# Download Istio
curl -L https://istio.io/downloadIstio | ISTIO_VERSION=1.20.0 sh -
cd istio-1.20.0
export PATH=$PWD/bin:$PATH

# Install Istio with the default profile
istioctl install --set profile=default -y

# Verify installation
istioctl verify-install
kubectl get pods -n istio-system
```

### 2. Configure Azure AKS Integration (Optional but Recommended)

For AKS clusters, consider using Azure's managed Istio add-on:

```bash
# Enable Istio add-on on existing AKS cluster
az aks mesh enable --resource-group <rg-name> --name <aks-name>

# Verify
az aks show --resource-group <rg-name> --name <aks-name> --query "serviceMeshProfile"
```

## Configuration Files

| File | Description |
|------|-------------|
| `peer-authentication.yaml` | mTLS policies (STRICT mode for all services) |
| `destination-rules.yaml` | Client-side TLS settings and traffic policies |
| `authorization-policies.yaml` | Service-to-service access control rules |

## Deployment

### Apply Service Mesh Configuration

```bash
# Apply to staging
kubectl apply -k infra/kubernetes/overlays/staging/

# Apply to production
kubectl apply -k infra/kubernetes/overlays/prod/

# Verify PeerAuthentication policies
kubectl get peerauthentication -n mys-staging
kubectl get peerauthentication -n mys-prod

# Verify DestinationRules
kubectl get destinationrule -n mys-staging
kubectl get destinationrule -n mys-prod

# Verify AuthorizationPolicies
kubectl get authorizationpolicy -n mys-staging
kubectl get authorizationpolicy -n mys-prod
```

### Restart Pods for Sidecar Injection

After enabling Istio injection, restart deployments to inject sidecars:

```bash
# Restart all deployments in the namespace
kubectl rollout restart deployment -n mys-staging
kubectl rollout restart statefulset -n mys-staging

# Verify sidecars are running (2/2 containers)
kubectl get pods -n mys-staging
```

## Security Policies

### mTLS Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| `STRICT` | Only accept mTLS traffic | Production (default) |
| `PERMISSIVE` | Accept both plain text and mTLS | Migration period |
| `DISABLE` | Disable mTLS | Not recommended |

### Authorization Policy Flow

1. **deny-all**: Default deny all traffic in the namespace
2. **allow-ingress-gateway**: Allow external traffic through Istio ingress
3. **Service-specific policies**: Fine-grained access control per service

### Service Communication Matrix

```
┌─────────────────┐      ┌─────────────────┐
│  Istio Ingress  │──────│   Admin UI      │
│    Gateway      │      └─────────────────┘
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌─────────────────┐
│   Admin API     │◄─────│   Publisher     │
└────────┬────────┘      └────────┬────────┘
         │                        │
         ▼                        ▼
┌─────────────────┐      ┌─────────────────┐
│ Story Generator │      │     Chain       │
└─────────────────┘      │   (RPC/WS)      │
                         └─────────────────┘
```

## Monitoring mTLS

### Verify mTLS is Working

```bash
# Check if traffic is encrypted
istioctl proxy-config secret <pod-name> -n mys-staging

# View mTLS status for a service
istioctl authn tls-check <pod-name> mys-chain.mys-staging.svc.cluster.local

# Check Kiali dashboard for traffic visualization
kubectl port-forward svc/kiali -n istio-system 20001:20001
```

### Debug Connection Issues

```bash
# Check if sidecar is injected
kubectl get pod <pod-name> -n mys-staging -o jsonpath='{.spec.containers[*].name}'

# View Istio proxy logs
kubectl logs <pod-name> -n mys-staging -c istio-proxy

# Analyze configuration
istioctl analyze -n mys-staging
```

## Gradual Migration Path

### Phase 1: Permissive Mode (Migration)

For initial deployment, use PERMISSIVE mode to allow mixed traffic:

```yaml
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
  namespace: mystira
spec:
  mtls:
    mode: PERMISSIVE  # Accept both plain and mTLS
```

### Phase 2: Strict Mode (Production)

Once all services have sidecars, switch to STRICT:

```yaml
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
  namespace: mystira
spec:
  mtls:
    mode: STRICT  # Only accept mTLS
```

## Troubleshooting

### Common Issues

1. **Pods not getting sidecars**
   - Verify namespace label: `kubectl get ns mys-staging --show-labels`
   - Should have `istio-injection=enabled`

2. **Connection refused after enabling STRICT**
   - Ensure all communicating services have sidecars
   - Check AuthorizationPolicies allow the connection

3. **Health checks failing**
   - Health check endpoints are excluded via AuthorizationPolicy
   - Verify kubelet can reach `/health` and `/ready` paths

4. **External services not reachable**
   - Create ServiceEntry for external services
   - Or use DestinationRule with `DISABLE` TLS mode

### Useful Commands

```bash
# List all Istio resources
kubectl get all -n istio-system

# Check mesh configuration
istioctl proxy-config all <pod-name> -n mys-staging

# Export proxy config for debugging
istioctl proxy-config dump <pod-name> -n mys-staging

# View metrics
kubectl port-forward svc/prometheus -n istio-system 9090:9090
```

## Related Documentation

- [Istio Documentation](https://istio.io/latest/docs/)
- [Azure AKS Istio Add-on](https://learn.microsoft.com/en-us/azure/aks/istio-about)
- [Mystira Security Policy](../../../SECURITY.md)
- [Chain Module Security](../../../terraform/modules/chain/README.md)
