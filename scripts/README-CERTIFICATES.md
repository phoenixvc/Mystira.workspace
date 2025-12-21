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

### Step 4: Clear Browser Cache and SSL State

Even after certificates are valid, browsers cache old certificates and SSL state. **This is the most common reason why you still see certificate errors after importing the staging CA.**

#### Quick Method (Try First)

**Chrome/Edge/Brave:**
1. Navigate to: `chrome://settings/clearBrowserData`
2. Select:
   - ✅ **Cached images and files**
   - ✅ **Cookies and other site data**
3. Time range: **Last hour** (or "All time")
4. Click **"Clear data"**
5. **Close ALL browser windows completely**
6. Restart browser

**Firefox:**
1. Press `Ctrl+Shift+Delete`
2. Select "Cache" and "Cookies"
3. Time range: "Last hour"
4. Click "Clear Now"
5. Close all Firefox windows
6. Restart Firefox

**Safari:**
1. Safari → Clear History
2. Select "Last hour"
3. Click "Clear History"
4. Quit Safari completely (Cmd+Q)
5. Restart Safari

#### Deep SSL State Clear (If Quick Method Doesn't Work)

If you still see certificate errors after clearing cache and restarting, the browser's SSL state cache may be holding onto old certificate information:

**Chrome/Edge/Brave - Advanced SSL State Clear:**

1. **Clear SSL state cache:**
   - Navigate to: `chrome://net-internals/#ssl`
   - Click **"Clear all"** button
   - This clears all cached SSL session data

2. **Close idle sockets:**
   - Navigate to: `chrome://net-internals/#sockets`
   - Click **"Close idle sockets"** button
   - Click **"Flush socket pools"** button
   - This forces new SSL connections

3. **Clear HSTS settings (if site was previously accessed):**
   - Navigate to: `chrome://net-internals/#hsts`
   - Under "Delete domain security policies"
   - Enter: `dev.story-generator.mystira.app`
   - Click **"Delete"**
   - Repeat for:
     - `dev.publisher.mystira.app`
     - `dev.chain.mystira.app`

4. **Verify Windows trusts the certificate:**
   ```bash
   # In Git Bash or PowerShell:
   curl -I https://dev.story-generator.mystira.app

   # If successful with no certificate errors:
   # - Windows trusts the certificate ✓
   # - Problem is browser cache/SSL state

   # If certificate errors:
   # - Certificate not imported correctly
   # - Re-import to Trusted Root Certification Authorities
   ```

5. **Complete browser restart:**
   - Close **ALL** browser windows and tabs
   - Open Task Manager (`Ctrl+Shift+Esc`)
   - Find browser process (chrome.exe, msedge.exe, brave.exe)
   - Right-click → **End Task** (if still running)
   - Wait 5 seconds
   - Restart browser

6. **Test:**
   - Navigate to: `https://dev.story-generator.mystira.app`
   - You should now see a green padlock with no warnings!

**Firefox - Advanced Cache Clear:**

1. Navigate to: `about:preferences#privacy`
2. Under "Cookies and Site Data", click **"Clear Data..."**
3. Check both boxes, click **"Clear"**
4. Navigate to: `about:networking#sockets`
5. Find any connections to `mystira.app` and close them
6. Close all Firefox windows
7. Restart Firefox

**Edge-Specific - Internet Options Method:**

1. Press `Win + R`
2. Type: `inetcpl.cpl`
3. Go to **"Content"** tab
4. Click **"Clear SSL state"** button
5. Click **"OK"**
6. Restart Edge

#### Verification After Clearing Cache

Run these tests to verify everything works:

```bash
# Test 1: Verify Windows trusts the certificate
curl -I https://dev.story-generator.mystira.app
# Expected: HTTP 200 OK (or 301/302), no certificate errors

# Test 2: Check certificate details
echo | openssl s_client -servername dev.story-generator.mystira.app \
  -connect dev.story-generator.mystira.app:443 2>/dev/null \
  | openssl x509 -noout -issuer -subject

# Expected output:
# issuer=C=US, O=(STAGING) Let's Encrypt, CN=(STAGING) Tenuous Tomato R13
# subject=CN=dev.story-generator.mystira.app
```

**Alternative: Use Incognito/Private Mode for Quick Testing**

Incognito mode doesn't use cached certificates, making it useful for testing:
- Chrome/Edge: `Ctrl+Shift+N`
- Firefox: `Ctrl+Shift+P`
- Safari: `Cmd+Shift+N`

Note: You may still need to import the certificate in incognito mode depending on browser settings.

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
