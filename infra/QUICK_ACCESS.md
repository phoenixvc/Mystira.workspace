# Quick Access - Publisher URLs

Quick reference for accessing Mystira Publisher across environments.

## Access URLs

### Publisher Service

| Environment     | URL                                   | SSL Certificate            | Status Check                                        |
| --------------- | ------------------------------------- | -------------------------- | --------------------------------------------------- |
| **Development** | https://dev.publisher.mystira.app     | Let's Encrypt Staging ⚠️    | `curl https://dev.publisher.mystira.app/health`     |
| **Staging**     | https://staging.publisher.mystira.app | Let's Encrypt Staging ⚠️    | `curl https://staging.publisher.mystira.app/health` |
| **Production**  | https://publisher.mystira.app         | Let's Encrypt Production ✅ | `curl https://publisher.mystira.app/health`         |

### Chain Service

| Environment     | URL                               | SSL Certificate            | Status Check                                     |
| --------------- | --------------------------------- | -------------------------- | ------------------------------------------------ |
| **Development** | https://dev.chain.mystira.app     | Let's Encrypt Staging ⚠️    | `curl -X POST https://dev.chain.mystira.app`     |
| **Staging**     | https://staging.chain.mystira.app | Let's Encrypt Staging ⚠️    | `curl -X POST https://staging.chain.mystira.app` |
| **Production**  | https://chain.mystira.app         | Let's Encrypt Production ✅ | `curl -X POST https://chain.mystira.app`         |

⚠️ **Note:** Dev and Staging use Let's Encrypt Staging certificates, which will show browser security warnings. This is intentional.

## Quick Setup Commands

### Check if Services are Running

```bash
# Check all environments
for env in dev staging prod; do
  ns="mys-$env"
  echo "=== $env environment ==="
  kubectl get pods -n $ns -l app=mystira-publisher
  kubectl get ingress -n $ns
  echo ""
done
```

### View Logs

```bash
# Dev
kubectl logs -n mys-dev -l app=mystira-publisher --tail=100 -f

# Staging
kubectl logs -n mys-staging -l app=mystira-publisher --tail=100 -f

# Production
kubectl logs -n mys-prod -l app=mystira-publisher --tail=100 -f
```

### Check Certificate Status

```bash
# Dev
kubectl get certificate -n mys-dev

# Staging
kubectl get certificate -n mys-staging

# Production
kubectl get certificate -n mys-prod
```

### Port Forward (Local Development)

If you need to access services locally without going through the public URL:

```bash
# Port forward Publisher (dev)
kubectl port-forward -n mys-dev svc/mystira-publisher 3000:3000

# Port forward Publisher (staging)
kubectl port-forward -n mys-staging svc/mystira-publisher 3000:3000

# Port forward Publisher (production)
kubectl port-forward -n mys-prod svc/mystira-publisher 3000:3000
```

Then access at: http://localhost:3000

### Restart Services

```bash
# Restart Publisher in dev
kubectl rollout restart deployment/mys-publisher -n mys-dev

# Restart Publisher in staging
kubectl rollout restart deployment/mys-publisher -n mys-staging

# Restart Publisher in production
kubectl rollout restart deployment/mystira-publisher -n mys-prod
```

## Browser Access

Simply open these URLs in your browser:

- **Development:** https://dev.publisher.mystira.app
  - ⚠️ You'll see a security warning - click "Advanced" → "Proceed" (staging certificate)
  
- **Staging:** https://staging.publisher.mystira.app
  - ⚠️ You'll see a security warning - click "Advanced" → "Proceed" (staging certificate)
  
- **Production:** https://publisher.mystira.app
  - ✅ No warnings - fully trusted certificate

## DNS Verification

Check if DNS is properly configured:

```bash
# Check dev
nslookup dev.publisher.mystira.app

# Check staging
nslookup staging.publisher.mystira.app

# Check production
nslookup publisher.mystira.app
```

All should return the appropriate load balancer IP for their environment.

## Troubleshooting Common Issues

### "Cannot resolve hostname"

DNS records haven't propagated yet or aren't configured. Wait up to 48 hours or check Terraform DNS configuration.

```bash
# Check if DNS records exist in Azure
az network dns record-set list \
  --resource-group <rg-name> \
  --zone-name mystira.app \
  --query "[?name=='dev.publisher' || name=='staging.publisher' || name=='publisher']"
```

### "Connection refused"

Service is not running or ingress is not properly configured.

```bash
# Check if pods are running
kubectl get pods -n mys-<env> -l app=mystira-publisher

# Check ingress configuration
kubectl describe ingress -n mys-<env> mystira-publisher-ingress
```

### "Certificate error" (Production only)

If production shows certificate errors, check cert-manager:

```bash
# Check certificate status
kubectl describe certificate mystira-publisher-tls -n mys-prod

# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager --tail=50
```

### "502 Bad Gateway"

Backend service is down or unhealthy.

```bash
# Check pod status
kubectl get pods -n mys-<env> -l app=mystira-publisher

# Check pod logs
kubectl logs -n mys-<env> -l app=mystira-publisher --tail=100

# Check service endpoints
kubectl get endpoints -n mys-<env> mystira-publisher
```

## Need Help?

For detailed setup instructions, see:
- [ENVIRONMENT_URLS_SETUP.md](./ENVIRONMENT_URLS_SETUP.md) - Complete setup guide
- [DNS_INGRESS_SETUP.md](./DNS_INGRESS_SETUP.md) - Original DNS/Ingress setup guide
- [README.md](./README.md) - Infrastructure overview
