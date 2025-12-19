# Environment-Specific URLs Configuration - Summary

## What Was Changed

Your infrastructure has been configured to support environment-specific URLs for the Publisher and Chain services.

### URL Structure

| Environment    | Publisher URL                           | Chain URL                           |
| -------------- | --------------------------------------- | ----------------------------------- |
| **Dev**        | `https://dev.publisher.mystira.app`     | `https://dev.chain.mystira.app`     |
| **Staging**    | `https://staging.publisher.mystira.app` | `https://staging.chain.mystira.app` |
| **Production** | `https://publisher.mystira.app`         | `https://chain.mystira.app`         |

## Files Modified

### 1. DNS Module (`terraform/modules/dns/main.tf`)

**Changes:**
- Added logic to create environment-specific subdomains
- Dev environment creates: `dev.publisher` and `dev.chain`
- Staging environment creates: `staging.publisher` and `staging.chain`
- Production environment creates: `publisher` and `chain` (no prefix)
- Updated FQDN outputs to reflect environment-specific domains

**Key Logic:**
```terraform
locals {
  publisher_subdomain = var.environment == "prod" ? "publisher" : "${var.environment}.publisher"
  chain_subdomain     = var.environment == "prod" ? "chain" : "${var.environment}.chain"
}
```

### 2. Kubernetes Dev Overlay (`kubernetes/overlays/dev/kustomization.yaml`)

**Changes:**
- Added ingress patches to override Publisher hostname to `dev.publisher.mystira.app`
- Added ingress patches to override Chain hostname to `dev.chain.mystira.app`
- Changed SSL issuer to `letsencrypt-staging` (avoids rate limits during dev)

### 3. Kubernetes Staging Overlay (`kubernetes/overlays/staging/kustomization.yaml`)

**Changes:**
- Added ingress patches to override Publisher hostname to `staging.publisher.mystira.app`
- Added ingress patches to override Chain hostname to `staging.chain.mystira.app`
- Changed SSL issuer to `letsencrypt-staging` (avoids rate limits during staging)

### 4. Kubernetes Production Overlay (`kubernetes/overlays/prod/kustomization.yaml`)

**No Changes:**
- Production uses the base configuration
- Hostname remains `publisher.mystira.app` and `chain.mystira.app`
- SSL issuer remains `letsencrypt-prod` (trusted certificates)

### 5. Documentation Updates

**New Files Created:**
- `ENVIRONMENT_URLS_SETUP.md` - Complete setup guide for environment-specific URLs
- `QUICK_ACCESS.md` - Quick reference for accessing services
- `ENVIRONMENT_URL_CHANGES_SUMMARY.md` - This file

**Modified Files:**
- `README.md` - Updated DNS configuration section with environment-specific URLs
- `DNS_INGRESS_SETUP.md` - Added reference to new environment-specific guide

## How to Deploy

### Option 1: Full Deploy (All Environments)

```bash
# 1. Deploy infrastructure for each environment
cd infra/terraform/environments/dev
terraform init && terraform apply

cd ../staging
terraform init && terraform apply

cd ../prod
terraform init && terraform apply

# 2. Get the load balancer IPs for each environment
kubectl get svc -n ingress-nginx nginx-ingress-ingress-nginx-controller

# 3. Update DNS records (edit each environment's main.tf with the IPs, then apply)
cd ../dev
# Edit main.tf to add publisher_ip and chain_ip
terraform apply

cd ../staging
# Edit main.tf to add publisher_ip and chain_ip
terraform apply

cd ../prod
# Edit main.tf to add publisher_ip and chain_ip
terraform apply

# 4. Deploy Kubernetes resources
cd ../../../kubernetes/overlays/dev
kubectl apply -k .

cd ../staging
kubectl apply -k .

cd ../prod
kubectl apply -k .
```

### Option 2: Quick Update (If Infrastructure Already Exists)

If your infrastructure is already deployed and you just want to update the ingress configuration:

```bash
# Just update the Kubernetes resources
cd infra/kubernetes/overlays/dev
kubectl apply -k .

cd ../staging
kubectl apply -k .

cd ../prod
kubectl apply -k .
```

### Option 3: Single Environment Update

If you only want to update dev environment:

```bash
cd infra/kubernetes/overlays/dev
kubectl apply -k .
```

## Testing Your Deployment

### 1. Verify DNS Resolution

```bash
# Check if DNS records exist
nslookup dev.publisher.mystira.app
nslookup staging.publisher.mystira.app
nslookup publisher.mystira.app
```

### 2. Check Ingress Configuration

```bash
# Dev
kubectl get ingress -n mys-dev -o yaml

# Staging
kubectl get ingress -n mys-staging -o yaml

# Production
kubectl get ingress -n mys-prod -o yaml
```

You should see the correct hostnames in the ingress rules.

### 3. Check SSL Certificates

```bash
# Dev (should show staging issuer)
kubectl get certificate -n mys-dev
kubectl describe certificate mystira-publisher-tls -n mys-dev | grep Issuer

# Staging (should show staging issuer)
kubectl get certificate -n mys-staging
kubectl describe certificate mystira-publisher-tls -n mys-staging | grep Issuer

# Production (should show production issuer)
kubectl get certificate -n mys-prod
kubectl describe certificate mystira-publisher-tls -n mys-prod | grep Issuer
```

### 4. Test Endpoints

```bash
# Dev
curl -k https://dev.publisher.mystira.app/health

# Staging
curl -k https://staging.publisher.mystira.app/health

# Production
curl https://publisher.mystira.app/health
```

Note: Use `-k` flag for dev/staging to bypass SSL certificate warnings (staging certificates).

## Access URLs Right Now

Once deployed, you can access your Publisher at:

- **Development:** https://dev.publisher.mystira.app
- **Staging:** https://staging.publisher.mystira.app  
- **Production:** https://publisher.mystira.app

## Important Notes

### SSL Certificates

1. **Dev and Staging:**
   - Use Let's Encrypt **Staging** certificates
   - Browsers will show security warnings ⚠️
   - This is intentional to avoid rate limits
   - Click "Advanced" → "Proceed anyway" in your browser

2. **Production:**
   - Uses Let's Encrypt **Production** certificates
   - Fully trusted by all browsers ✅
   - No warnings

### DNS Propagation

- DNS changes can take up to 48 hours to propagate globally
- Typically takes a few minutes to a few hours
- Check propagation status: https://www.whatsmydns.net/

### First-Time Setup

If this is your first time setting up:

1. **Domain Registration:** Ensure `mystira.app` is registered and you have access
2. **Name Servers:** After deploying production Terraform, update your domain registrar with Azure DNS name servers
3. **Load Balancer IPs:** Get the external IPs from NGINX ingress controllers in each cluster
4. **DNS Records:** Update Terraform with the IPs and apply to create DNS records

## Troubleshooting

### "DNS name not found"

**Cause:** DNS records haven't been created or haven't propagated yet.

**Solution:**
1. Check if DNS records exist in Azure:
   ```bash
   az network dns record-set list --resource-group <rg-name> --zone-name mystira.app
   ```
2. Wait for DNS propagation (up to 48 hours)
3. Verify nameservers are configured correctly at your domain registrar

### "Connection refused" or "502 Bad Gateway"

**Cause:** Service isn't running or ingress isn't properly configured.

**Solution:**
1. Check if pods are running:
   ```bash
   kubectl get pods -n mys-<env> -l app=mystira-publisher
   ```
2. Check pod logs:
   ```bash
   kubectl logs -n mys-<env> -l app=mystira-publisher
   ```
3. Verify ingress configuration:
   ```bash
   kubectl describe ingress -n mys-<env> mystira-publisher-ingress
   ```

### "Certificate not trusted" (Production only)

**Cause:** Certificate hasn't been issued yet or failed to issue.

**Solution:**
1. Check certificate status:
   ```bash
   kubectl describe certificate mystira-publisher-tls -n mys-prod
   ```
2. Check cert-manager logs:
   ```bash
   kubectl logs -n cert-manager -l app=cert-manager
   ```
3. Ensure DNS is resolving correctly (cert-manager needs this)

### Wrong URL showing up

**Cause:** Kustomize patches not applied correctly.

**Solution:**
1. Reapply the kustomization:
   ```bash
   kubectl apply -k infra/kubernetes/overlays/<env>
   ```
2. Force a rollout restart:
   ```bash
   kubectl rollout restart deployment/mys-publisher -n mys-<env>
   ```

## Next Steps

1. **Deploy the Changes:** Follow the deployment steps above
2. **Test Each Environment:** Use the testing section to verify everything works
3. **Update CI/CD:** Update your GitHub Actions workflows to use the correct URLs
4. **Set Up Monitoring:** Configure alerts for certificate expiration and service health
5. **Review Documentation:** Read through the detailed guides:
   - [ENVIRONMENT_URLS_SETUP.md](./ENVIRONMENT_URLS_SETUP.md) - Full setup guide
   - [QUICK_ACCESS.md](./QUICK_ACCESS.md) - Quick reference
   - [DNS_INGRESS_SETUP.md](./DNS_INGRESS_SETUP.md) - DNS setup details

## Questions?

- For detailed setup: See [ENVIRONMENT_URLS_SETUP.md](./ENVIRONMENT_URLS_SETUP.md)
- For quick commands: See [QUICK_ACCESS.md](./QUICK_ACCESS.md)
- For DNS details: See [DNS_INGRESS_SETUP.md](./DNS_INGRESS_SETUP.md)
- For infrastructure overview: See [README.md](./README.md)
