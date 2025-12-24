# SSL/TLS Certificates Guide

## Overview

This guide explains how SSL/TLS certificates work in the Mystira infrastructure and how to access dev and staging environments that use Let's Encrypt staging certificates.

## Certificate Strategy by Environment

| Environment | Certificate Issuer       | Browser Behavior          | Use Case                  |
| ----------- | ------------------------ | ------------------------- | ------------------------- |
| **dev**     | Let's Encrypt Staging    | ⚠️ Shows security warning | Development and testing   |
| **staging** | Let's Encrypt Staging    | ⚠️ Shows security warning | Pre-production validation |
| **prod**    | Let's Encrypt Production | ✅ Fully trusted          | Production services       |

## Understanding the Certificate Warning in Dev/Staging

### Why You See "Your connection is not private"

When accessing dev or staging environments, you'll see:

```
Your connection is not private
NET::ERR_CERT_AUTHORITY_INVALID
Attackers might be trying to steal your information from dev.*.mystira.app
```

**This is expected and normal behavior!** Here's why:

1. **Staging certificates are intentionally untrusted** by browsers to prevent accidental use in production
2. **Same validation process** as production certificates (domain ownership verification)
3. **Higher rate limits** - Let's Encrypt production has strict limits (50 certs/week), staging doesn't
4. **Free testing** - We can test certificate automation without risking production quota

### How to Access Dev/Staging Sites

**You can safely access dev/staging sites - here's how:**

1. When you see the security warning, click **"Advanced"** (or "Show Details")
2. Click **"Proceed to dev.\*.mystira.app (unsafe)"** or similar option
3. The site will load normally and function correctly

**This is the standard workflow for accessing dev/staging environments.**

## Certificate Architecture

### cert-manager Configuration

Our Kubernetes clusters use [cert-manager](https://cert-manager.io/) to automatically provision and renew SSL/TLS certificates.

**ClusterIssuers:**

```yaml
# Staging issuer (dev/staging environments)
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory
    email: admin@phoenixvc.org
    privateKeySecretRef:
      name: letsencrypt-staging
    solvers:
      - http01:
          ingress:
            class: nginx

# Production issuer (prod environment)
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@phoenixvc.org
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
      - http01:
          ingress:
            class: nginx
```

### Ingress Annotations

Each environment's ingress resources specify which issuer to use:

**Dev/Staging:**

```yaml
metadata:
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-staging"
```

**Production:**

```yaml
metadata:
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
```

## Certificate Lifecycle

### Automatic Provisioning

When an ingress is created or updated:

1. **cert-manager detects** the ingress with `cert-manager.io/cluster-issuer` annotation
2. **Certificate resource** is automatically created
3. **ACME HTTP01 challenge** is initiated to prove domain ownership
4. **Let's Encrypt validates** the challenge by making an HTTP request to the domain
5. **Certificate is issued** and stored in a Kubernetes secret
6. **NGINX ingress** automatically uses the certificate for HTTPS

**Timeline:** Typically takes 5-15 minutes from deployment to working certificate.

### Automatic Renewal

- **Certificates expire** after 90 days
- **cert-manager automatically renews** certificates 30 days before expiration
- **No manual intervention** required
- **Monitoring** via cert-manager metrics and Kubernetes events

## Accessing Different Environments

### Development Environment

**Domains:**

- `dev.publisher.mystira.app`
- `dev.chain.mystira.app`
- `dev.story-api.mystira.app`

**Certificate Type:** Let's Encrypt Staging

**Access Method:**

1. Navigate to `https://dev.*.mystira.app`
2. Click "Advanced" on security warning
3. Click "Proceed to dev.\*.mystira.app (unsafe)"

### Staging Environment

**Domains:**

- `staging.publisher.mystira.app`
- `staging.chain.mystira.app`
- `staging.story-api.mystira.app`

**Certificate Type:** Let's Encrypt Staging

**Access Method:** Same as dev environment

### Production Environment

**Domains:**

- `publisher.mystira.app`
- `chain.mystira.app`
- `story-api.mystira.app`

**Certificate Type:** Let's Encrypt Production

**Access Method:** Direct access - no security warnings

## Trusting Staging Certificates (Optional)

If you frequently access dev/staging environments and want to avoid clicking "Proceed" every time, you can import the Let's Encrypt staging root CA to your browser.

### Download Staging Root CA

```bash
curl -o letsencrypt-stg-root-x1.pem https://letsencrypt.org/certs/staging/letsencrypt-stg-root-x1.pem
```

### Import to Browser

#### Windows (Recommended Method for Chrome, Edge, Brave, Arc, etc.)

**All Chromium-based browsers on Windows use the Windows Certificate Store.** This is the most reliable method:

1. **Download the certificate:**

   ```bash
   curl -o ~/Downloads/letsencrypt-staging.crt https://letsencrypt.org/certs/staging/letsencrypt-stg-root-x1.pem
   ```

2. **Open Windows Certificate Manager:**
   - Press `Win + R`
   - Type: `certmgr.msc`
   - Press Enter

3. **Import the certificate:**
   - Navigate to: **Trusted Root Certification Authorities** → **Certificates**
   - Right-click on **Certificates** folder
   - Select **All Tasks** → **Import...**
   - Click **Next**
   - Click **Browse...** and navigate to your Downloads folder
   - **Important:** Change file filter to **"All Files (_._)"**
   - Select `letsencrypt-staging.crt`
   - Click **Next** → **Next** → **Finish**
   - You should see: "The import was successful"

4. **Verify the import:**
   - In the Certificates window, you should now see:
   - Certificate name: **(STAGING) Pretend Pear X1**
   - Issued To: **(STAGING) Pretend Pear X1**
   - Issued By: **(STAGING) Pretend Pear X1**

   ![Windows Certificate Manager showing the imported staging certificate](../assets/certmgr-staging-cert.png)

5. **Close ALL browser windows completely:**
   - Close all tabs and windows
   - Check Task Manager (Ctrl+Shift+Esc) to ensure the browser process is fully closed
   - Look for any browser processes and end them if they're still running

6. **Clear browser SSL state:**
   - Open browser
   - Navigate to: `chrome://settings/clearBrowserData` (or `edge://settings/clearBrowserData`)
   - Select:
     - ✅ Cached images and files
     - ✅ Cookies and other site data
   - Time range: "Last hour"
   - Click "Clear data"

7. **Restart browser and test:**
   - Navigate to `https://dev.story-api.mystira.app`
   - You should see a green padlock with no security warnings!

**Verification:** After import, run this in Git Bash:

```bash
curl -I https://dev.story-api.mystira.app
# If no certificate errors, the import worked!
```

#### Firefox (Windows, macOS, Linux)

Firefox uses its own certificate store, separate from the system:

1. Download the certificate (same as above)
2. Settings → Privacy & Security → Certificates
3. View Certificates → Authorities
4. Import → Select `letsencrypt-staging.crt`
5. Check "Trust this CA to identify websites"
6. Click OK
7. **Close ALL Firefox windows**
8. Restart Firefox

#### Safari (macOS)

1. Download: `curl -o ~/Downloads/letsencrypt-staging.crt https://letsencrypt.org/certs/staging/letsencrypt-stg-root-x1.pem`
2. Double-click `letsencrypt-staging.crt` to open Keychain Access
3. Find "(STAGING) Pretend Pear X1" in the System or Login keychain
4. Double-click the certificate
5. Expand "Trust" section
6. Set "When using this certificate" to: **Always Trust**
7. Close (enter password if prompted)
8. **Quit Safari completely** (Cmd+Q)
9. Restart Safari

### Troubleshooting Certificate Import

**Problem:** Still seeing certificate warning after import

**Solutions:**

1. **Browser not fully closed:**
   - Windows: Open Task Manager (Ctrl+Shift+Esc)
   - Look for browser processes (chrome.exe, msedge.exe, brave.exe, etc.)
   - End all browser processes
   - Restart browser

2. **Wrong certificate store:**
   - Certificate must be in **"Trusted Root Certification Authorities"**
   - NOT in "Personal" or "Intermediate Certification Authorities"
   - Re-import to correct location if needed

3. **Browser cache (CRITICAL - Most Common Issue):**

   **Standard cache clear:**
   - Navigate to: `chrome://settings/clearBrowserData` (or `edge://settings/clearBrowserData`)
   - Select:
     - ✅ **Cached images and files**
     - ✅ **Cookies and other site data**
   - Time range: **Last hour** (or "All time" to be thorough)
   - Click **"Clear data"**
   - **Close browser completely**
   - Restart browser

   **Deep SSL state clear (if standard clear doesn't work):**

   Step 1 - Clear SSL state:
   - Navigate to: `chrome://net-internals/#ssl`
   - Click **"Clear all"** button

   Step 2 - Close idle sockets:
   - Navigate to: `chrome://net-internals/#sockets`
   - Click **"Close idle sockets"** button
   - Click **"Flush socket pools"** button

   Step 3 - Clear HSTS settings (if applicable):
   - Navigate to: `chrome://net-internals/#hsts`
   - Under "Delete domain security policies"
   - Enter: `dev.story-api.mystira.app`
   - Click **"Delete"**
   - Repeat for other dev domains

   Step 4 - Complete restart:
   - Close ALL browser windows
   - Check Task Manager - end all browser processes
   - Wait 5 seconds
   - Restart browser
   - Test: `https://dev.story-api.mystira.app`

   **Verify Windows trusts the certificate:**

   ```bash
   # In Git Bash, test if Windows trusts the cert:
   curl -I https://dev.story-api.mystira.app

   # If no certificate errors, the import worked!
   # If still errors, the certificate isn't imported correctly
   ```

4. **Multiple certificate chain needed:**

   ```bash
   # Download and import both staging roots:
   curl -o ~/Downloads/staging-root-x1.crt https://letsencrypt.org/certs/staging/letsencrypt-stg-root-x1.pem
   curl -o ~/Downloads/staging-root-x2.crt https://letsencrypt.org/certs/staging/letsencrypt-stg-root-x2.pem
   # Import both using certmgr.msc
   ```

5. **DER format may be required:**
   ```bash
   # Convert to DER format (Windows native)
   openssl x509 -in ~/Downloads/letsencrypt-staging.crt -outform der -out ~/Downloads/letsencrypt-staging.der
   # Import the .der file instead
   ```

**After successful import:** dev._.mystira.app and staging._.mystira.app will show as trusted with green padlock icon.

⚠️ **Security Warning:** Only import staging certificates on development machines. Never import them on production systems or shared computers.

## Troubleshooting

### Check Certificate Status

```bash
# List all certificates in a namespace
kubectl get certificates -n mys-dev

# Check specific certificate details
kubectl describe certificate mystira-story-generator-tls-dev -n mys-dev

# View certificate from secret
kubectl get secret mystira-story-generator-tls-dev -n mys-dev \
  -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -noout -text
```

### Check Certificate Issuer

```bash
# Verify it's using staging issuer
kubectl get secret mystira-story-generator-tls-dev -n mys-dev \
  -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -noout -issuer

# Expected output for staging:
# issuer=C=US, O=(STAGING) Let's Encrypt, CN=(STAGING) Tenuous Tomato R13
```

### Common Issues

#### Certificate Not Ready

**Symptom:**

```bash
kubectl get certificates -n mys-dev
NAME                              READY   SECRET                            AGE
mystira-story-generator-tls-dev   False   mystira-story-generator-tls-dev   2m
```

**Solutions:**

1. Wait 5-10 minutes - certificate issuance takes time
2. Check cert-manager logs: `kubectl logs -n cert-manager deployment/cert-manager`
3. Check certificate events: `kubectl describe certificate mystira-story-generator-tls-dev -n mys-dev`
4. Verify domain is accessible from internet (cert-manager needs to validate)

#### Wrong Certificate Issuer

**Symptom:** Certificate shows "Let's Encrypt Authority X3" instead of "STAGING"

**Solutions:**

```bash
# Check ingress annotation
kubectl get ingress mys-story-generator-ingress -n mys-dev \
  -o jsonpath='{.metadata.annotations.cert-manager\.io/cluster-issuer}'

# Should output: letsencrypt-staging

# If wrong, update the ingress annotation and delete the certificate
kubectl delete certificate mystira-story-generator-tls-dev -n mys-dev
kubectl delete secret mystira-story-generator-tls-dev -n mys-dev
# cert-manager will recreate with correct issuer
```

#### Domain Not Resolving

**Symptom:** `nslookup dev.story-api.mystira.app` returns no records

**Solutions:**

```bash
# Check Azure DNS zone
az network dns record-set a list \
  --zone-name mystira.app \
  --resource-group mys-prod-core-rg-glob

# Get ingress IP
kubectl get ingress mys-story-generator-ingress -n mys-dev \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}'

# Add DNS record if missing
az network dns record-set a add-record \
  --resource-group mys-prod-core-rg-glob \
  --zone-name mystira.app \
  --record-set-name dev.story-generator \
  --ipv4-address <INGRESS_IP>
```

## Debugging Tools

### Automated Debug Script

Run the comprehensive certificate debugging script:

```bash
./scripts/debug-certificates.sh
```

This checks:

- Kubernetes connectivity
- DNS resolution
- NGINX ingress controller
- cert-manager health
- Certificate issuers
- Ingress resources
- Certificate status
- TLS secrets
- HTTPS connectivity

### Manual Certificate Check

```bash
# Test HTTPS connection
curl -vI https://dev.story-api.mystira.app

# Get certificate details
echo | openssl s_client -servername dev.story-api.mystira.app \
  -connect dev.story-api.mystira.app:443 2>/dev/null \
  | openssl x509 -noout -text
```

## Security Considerations

### Why Not Use Production Certificates for Dev?

**Rate Limits:**

- Let's Encrypt production: 50 certificates per registered domain per week
- Frequent dev deployments could hit this limit
- Staging has much higher limits

**Risk of Misconfiguration:**

- Using production certs in dev increases risk of:
  - Accidentally exposing production credentials
  - Confusing dev and prod environments
  - Wasting production certificate quota on testing

**Best Practice:**

- Dev/Staging: Use staging certificates
- Production: Use production certificates
- Clear visual distinction (browser warning) prevents confusion

### Certificate Storage

**Kubernetes Secrets:**

- Certificates stored as Kubernetes secrets in each namespace
- Type: `kubernetes.io/tls`
- Contains: `tls.crt` (certificate) and `tls.key` (private key)
- Access controlled by Kubernetes RBAC

**Security Best Practices:**

- Private keys never leave the cluster
- Secrets encrypted at rest (Azure AKS encryption)
- No manual certificate management required
- Automatic rotation before expiration

## Rate Limits

### Let's Encrypt Production Limits

| Limit Type                         | Value                                          |
| ---------------------------------- | ---------------------------------------------- |
| Certificates per Registered Domain | 50 per week                                    |
| Duplicate Certificate              | 5 per week                                     |
| Failed Validations                 | 5 failures per account, per hostname, per hour |
| Accounts per IP Address            | 10 per 3 hours                                 |
| Pending Authorizations             | 300 per account                                |

### Let's Encrypt Staging Limits

Staging environment has **much higher** rate limits suitable for testing and development.

## Further Reading

- [cert-manager Documentation](https://cert-manager.io/docs/)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [Let's Encrypt Staging Environment](https://letsencrypt.org/docs/staging-environment/)
- [ACME Protocol](https://tools.ietf.org/html/rfc8555)
- [Certificate Troubleshooting Guide](../../scripts/README-CERTIFICATES.md)

## Related Documentation

- [Infrastructure Overview](./README.md)
- [Kubernetes Troubleshooting](./troubleshooting-kubernetes-center.md)
- [Infrastructure Deployment Guide](./quick-start-deploy.md)
