# Certificate Debugging Guide

## Quick Start

After deploying the certificate fixes, run the debugging script:

```bash
# First, configure kubectl to connect to your AKS cluster
az aks get-credentials --resource-group mys-dev-mystira-rg-san --name mys-dev-mystira-aks-san

# Run the debug script
./scripts/debug-certificates.sh
```

## What the Script Checks

The script performs comprehensive checks:

1. ✅ **Kubernetes Connection** - Verifies kubectl can connect to AKS
2. ✅ **Namespace** - Confirms mys-dev namespace exists
3. ✅ **DNS Resolution** - Checks if domains resolve to correct IPs
4. ✅ **NGINX Ingress Controller** - Verifies ingress controller is running
5. ✅ **cert-manager** - Checks cert-manager pods are healthy
6. ✅ **Certificate Issuers** - Validates Let's Encrypt issuers (staging & prod)
7. ✅ **Ingress Resources** - Inspects ingress configurations and annotations
8. ✅ **Certificate Resources** - Checks cert-manager Certificate objects
9. ✅ **TLS Secrets** - Examines actual certificate data and expiration
10. ✅ **cert-manager Logs** - Shows recent errors from cert-manager
11. ✅ **Certificate Requests** - Lists recent certificate issuance attempts
12. ✅ **HTTPS Connection Test** - Tests actual HTTPS connectivity

## Deployment Workflow

### Step 1: Deploy the Configuration

**Option A - Merge PR and Auto-Deploy:**
```bash
# Merge the PR on GitHub
# Wait for GitHub Actions to complete deployment
```

**Option B - Manual Deployment:**
```bash
# Trigger the workflow manually from GitHub Actions UI:
# 1. Go to Actions → Infrastructure Deploy
# 2. Click "Run workflow"
# 3. Select environment: dev
# 4. Select components: kubernetes-only
```

**Option C - Direct kubectl Apply (Advanced):**
```bash
# Configure kubectl
az aks get-credentials --resource-group mys-dev-mystira-rg-san --name mys-dev-mystira-aks-san

# Apply the dev overlay
kubectl apply -k infra/kubernetes/overlays/dev/
```

### Step 2: Wait for Certificate Issuance

After deployment, cert-manager needs time to issue certificates:

```bash
# Watch certificate status (should show "Ready: True" after 2-5 minutes)
kubectl get certificates -n mys-dev -w

# Check specific certificate
kubectl describe certificate mystira-story-generator-tls-dev -n mys-dev
```

### Step 3: Run Debug Script

```bash
./scripts/debug-certificates.sh
```

### Step 4: Clear Browser Cache

Even after certificates are valid, browsers cache old certificates:

**Chrome/Edge:**
1. Open DevTools (F12)
2. Right-click refresh button → "Empty Cache and Hard Reload"
3. Or: Settings → Privacy → Clear browsing data → Cached images and files

**Firefox:**
1. Ctrl+Shift+Delete
2. Select "Cache" and "Cookies"
3. Clear

**Safari:**
1. Develop → Empty Caches
2. Or: Safari → Clear History

**Alternative: Use Incognito/Private Mode**

## Common Issues & Solutions

### Issue 1: Certificate Not Ready

**Symptoms:**
```
kubectl get certificates -n mys-dev
NAME                              READY   SECRET                            AGE
mystira-story-generator-tls-dev   False   mystira-story-generator-tls-dev   2m
```

**Solutions:**
```bash
# Check certificate details
kubectl describe certificate mystira-story-generator-tls-dev -n mys-dev

# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager --tail=50

# Common fixes:
# 1. Wait longer (can take up to 10 minutes)
# 2. Check if domain is accessible from internet (cert-manager needs to verify)
# 3. Delete and recreate certificate:
kubectl delete certificate mystira-story-generator-tls-dev -n mys-dev
kubectl delete secret mystira-story-generator-tls-dev -n mys-dev
# Wait for cert-manager to recreate them
```

### Issue 2: Wrong Certificate Issuer

**Symptoms:**
- Browser shows valid certificate but from wrong issuer
- Certificate shows "Let's Encrypt Authority X3" instead of "Staging"

**Solutions:**
```bash
# Check ingress annotation
kubectl get ingress mystira-story-generator-ingress -n mys-dev -o yaml | grep cert-manager

# Should show: cert-manager.io/cluster-issuer: letsencrypt-staging
# If not, redeploy the configuration

# Force certificate recreation with correct issuer:
kubectl delete certificate mystira-story-generator-tls-dev -n mys-dev
kubectl delete secret mystira-story-generator-tls-dev -n mys-dev
kubectl annotate ingress mystira-story-generator-ingress -n mys-dev cert-manager.io/cluster-issuer=letsencrypt-staging --overwrite
```

### Issue 3: DNS Not Resolving

**Symptoms:**
```bash
nslookup dev.story-generator.mystira.app
# Returns: NXDOMAIN or no records
```

**Solutions:**
```bash
# Check Azure DNS zone
az network dns record-set a list --zone-name mystira.app --resource-group mys-prod-mystira-rg-glob

# Get ingress IP
kubectl get ingress -n mys-dev

# Add/update DNS record if missing
INGRESS_IP=$(kubectl get ingress mystira-story-generator-ingress -n mys-dev -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
az network dns record-set a add-record \
  --resource-group mys-prod-mystira-rg-glob \
  --zone-name mystira.app \
  --record-set-name dev.story-generator \
  --ipv4-address $INGRESS_IP
```

### Issue 4: Ingress Has No External IP

**Symptoms:**
```bash
kubectl get ingress -n mys-dev
# ADDRESS column is empty
```

**Solutions:**
```bash
# Check NGINX ingress controller
kubectl get svc -n ingress-nginx

# If LoadBalancer service has no external IP, check Azure
az aks show --resource-group mys-dev-mystira-rg-san --name mys-dev-mystira-aks-san --query "networkProfile.loadBalancerSku"

# Restart ingress controller
kubectl rollout restart deployment ingress-nginx-controller -n ingress-nginx
```

### Issue 5: cert-manager Not Working

**Symptoms:**
- Certificates stuck in "False" state
- No certificate requests being created

**Solutions:**
```bash
# Check cert-manager pods
kubectl get pods -n cert-manager

# Check ClusterIssuers
kubectl get clusterissuer
kubectl describe clusterissuer letsencrypt-staging

# Check cert-manager logs for errors
kubectl logs -n cert-manager deployment/cert-manager -f

# If cert-manager is broken, redeploy it:
kubectl apply -k infra/kubernetes/base/cert-manager/
```

## Manual Certificate Commands

### Force Certificate Renewal
```bash
# Delete certificate and secret
kubectl delete certificate mystira-story-generator-tls-dev -n mys-dev
kubectl delete secret mystira-story-generator-tls-dev -n mys-dev

# cert-manager will automatically recreate them
# Watch the process:
kubectl get certificates -n mys-dev -w
```

### Check Certificate Details
```bash
# Get certificate info from secret
kubectl get secret mystira-story-generator-tls-dev -n mys-dev -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -noout -text

# Check expiration
kubectl get secret mystira-story-generator-tls-dev -n mys-dev -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -noout -enddate

# Check issuer
kubectl get secret mystira-story-generator-tls-dev -n mys-dev -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -noout -issuer
```

### Test Certificate from Command Line
```bash
# Test HTTPS connection
curl -vI https://dev.story-generator.mystira.app

# Get certificate info
echo | openssl s_client -servername dev.story-generator.mystira.app -connect dev.story-generator.mystira.app:443 2>/dev/null | openssl x509 -noout -text
```

### Check Certificate Events
```bash
# See what cert-manager is doing
kubectl describe certificate mystira-story-generator-tls-dev -n mys-dev

# Check certificate request
kubectl get certificaterequest -n mys-dev
kubectl describe certificaterequest <request-name> -n mys-dev

# Check ACME challenges (if using HTTP01)
kubectl get challenges -n mys-dev
```

## Understanding Let's Encrypt Staging vs Production

**Staging Certificates (letsencrypt-staging):**
- ✅ Used for dev/staging environments
- ✅ Higher rate limits (no risk of being blocked)
- ✅ Same validation process as production
- ⚠️ Not trusted by browsers (will show warning)
- ⚠️ But you can click "Advanced" → "Proceed" for testing

**Production Certificates (letsencrypt-prod):**
- ✅ Trusted by all browsers
- ✅ No security warnings
- ⚠️ Strict rate limits (50 certs per domain per week)
- ⚠️ Should only be used for production domains

**Why dev uses staging:**
- Avoids rate limits during testing
- Same validation process verifies configuration works
- Can switch to prod for production environment

## Expected Browser Behavior

### With Staging Certificates (Current Fix)
1. Browser shows security warning: "Your connection is not private"
2. Error: NET::ERR_CERT_AUTHORITY_INVALID
3. Click "Advanced" → "Proceed to dev.story-generator.mystira.app (unsafe)"
4. Site loads normally
5. This is EXPECTED behavior for dev environment with staging certs

### With Production Certificates (Not Recommended for Dev)
1. No browser warning
2. Green padlock icon
3. Certificate is trusted
4. But risks hitting rate limits during testing

## Timeline After Deployment

| Time | What's Happening |
|------|------------------|
| 0 min | Deploy configuration to Kubernetes |
| 1-2 min | Ingress resources updated |
| 2-3 min | cert-manager detects new ingress annotations |
| 3-5 min | cert-manager creates Certificate resources |
| 5-7 min | cert-manager initiates ACME HTTP01 challenge |
| 7-10 min | Let's Encrypt validates domain ownership |
| 10-12 min | Certificate issued and stored in Secret |
| 12+ min | NGINX ingress starts using new certificate |

**Total Time: 10-15 minutes from deployment to working certificate**

## Quick Reference

```bash
# Most common commands you'll need:

# 1. Check if certificates are ready
kubectl get certificates -n mys-dev

# 2. Get detailed certificate status
kubectl describe certificate mystira-story-generator-tls-dev -n mys-dev

# 3. Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager --tail=50

# 4. Force certificate renewal
kubectl delete certificate mystira-story-generator-tls-dev -n mys-dev
kubectl delete secret mystira-story-generator-tls-dev -n mys-dev

# 5. Test HTTPS
curl -vI https://dev.story-generator.mystira.app

# 6. Run full debug
./scripts/debug-certificates.sh
```

## Support

If the debug script shows errors you can't resolve:

1. Check the cert-manager documentation: https://cert-manager.io/docs/
2. Review GitHub Actions logs for deployment errors
3. Check Azure AKS cluster health in Azure Portal
4. Verify DNS records in Azure DNS zone

## Files Modified

This fix modified the following file:
- `infra/kubernetes/overlays/dev/kustomization.yaml`

Key changes:
- Added `cert-manager.io/cluster-issuer: letsencrypt-staging` to publisher ingress
- Added `cert-manager.io/cluster-issuer: letsencrypt-staging` to chain ingress
- Added complete ingress patch for story-generator with staging certificate configuration
