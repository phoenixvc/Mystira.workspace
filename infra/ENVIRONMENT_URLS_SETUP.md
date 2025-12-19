# Environment-Specific URLs Setup Guide

This guide explains how to access the Publisher and Chain services with environment-specific URLs.

## URL Structure

The infrastructure is now configured with environment-specific subdomains:

| Environment | Publisher URL                           | Chain URL                           | SSL Certificate          |
| ----------- | --------------------------------------- | ----------------------------------- | ------------------------ |
| **Dev**     | `https://dev.publisher.mystira.app`     | `https://dev.chain.mystira.app`     | Let's Encrypt Staging    |
| **Staging** | `https://staging.publisher.mystira.app` | `https://staging.chain.mystira.app` | Let's Encrypt Staging    |
| **Prod**    | `https://publisher.mystira.app`         | `https://chain.mystira.app`         | Let's Encrypt Production |

## Prerequisites

1. Azure subscription with appropriate permissions
2. Control of the `mystira.app` domain at your domain registrar
3. kubectl configured with access to your AKS clusters
4. Terraform >= 1.5.0 installed
5. Helm 3.x installed

## Setup Steps

### Step 1: Deploy Infrastructure for Each Environment

Deploy infrastructure for each environment. The DNS module will automatically create the correct subdomain based on the environment:

#### Dev Environment

```bash
cd infra/terraform/environments/dev
terraform init
terraform plan
terraform apply
```

#### Staging Environment

```bash
cd infra/terraform/environments/staging
terraform init
terraform plan
terraform apply
```

#### Production Environment

```bash
cd infra/terraform/environments/prod
terraform init
terraform plan
terraform apply
```

### Step 2: Get Name Servers (Production Only)

After deploying production, get the Azure DNS name servers:

```bash
cd infra/terraform/environments/prod
terraform output dns_name_servers
```

You'll get output like:

```
[
  "ns1-01.azure-dns.com.",
  "ns2-01.azure-dns.net.",
  "ns3-01.azure-dns.org.",
  "ns4-01.azure-dns.info."
]
```

### Step 3: Configure Domain Registrar (One-Time Setup)

Log into your domain registrar where `mystira.app` is registered and:

1. Navigate to DNS settings for `mystira.app`
2. Replace the existing nameservers with the Azure DNS nameservers from Step 2
3. Save the changes

**Note:** DNS propagation can take up to 48 hours but typically completes within a few hours.

### Step 4: Install NGINX Ingress Controller (Per Cluster)

Install the NGINX ingress controller in each AKS cluster:

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# For Dev
helm install nginx-ingress ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz \
  --kubeconfig ~/.kube/dev-config

# For Staging
helm install nginx-ingress ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz \
  --kubeconfig ~/.kube/staging-config

# For Production
helm install nginx-ingress ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz \
  --kubeconfig ~/.kube/prod-config
```

Wait for the load balancer to be provisioned:

```bash
kubectl get svc -n ingress-nginx nginx-ingress-ingress-nginx-controller -w
```

Note the external IP address assigned for each environment.

### Step 5: Install cert-manager (Per Cluster)

Install cert-manager for automated SSL certificate management in each cluster:

```bash
# Apply to each cluster
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Wait for cert-manager to be ready
kubectl wait --for=condition=ready pod -l app.kubernetes.io/instance=cert-manager -n cert-manager --timeout=300s
```

### Step 6: Update DNS Records with Load Balancer IPs

Once you have the load balancer external IP for each environment, update the Terraform configuration:

#### Dev Environment

```bash
cd infra/terraform/environments/dev

# Edit main.tf and update the DNS module configuration:
# module "dns" {
#   source = "../../modules/dns"
#   ...
#   publisher_ip = "<DEV_LOAD_BALANCER_IP>"
#   chain_ip     = "<DEV_LOAD_BALANCER_IP>"
# }

terraform plan
terraform apply
```

This will create DNS A records for:
- `dev.publisher.mystira.app`
- `dev.chain.mystira.app`

#### Staging Environment

```bash
cd infra/terraform/environments/staging

# Edit main.tf and update the DNS module configuration:
# module "dns" {
#   source = "../../modules/dns"
#   ...
#   publisher_ip = "<STAGING_LOAD_BALANCER_IP>"
#   chain_ip     = "<STAGING_LOAD_BALANCER_IP>"
# }

terraform plan
terraform apply
```

This will create DNS A records for:
- `staging.publisher.mystira.app`
- `staging.chain.mystira.app`

#### Production Environment

```bash
cd infra/terraform/environments/prod

# Edit main.tf and update the DNS module configuration:
# module "dns" {
#   source = "../../modules/dns"
#   ...
#   publisher_ip = "<PROD_LOAD_BALANCER_IP>"
#   chain_ip     = "<PROD_LOAD_BALANCER_IP>"
# }

terraform plan
terraform apply
```

This will create DNS A records for:
- `publisher.mystira.app`
- `chain.mystira.app`

### Step 7: Deploy Kubernetes Resources

Deploy the Kubernetes resources including ingress and cert-manager configuration for each environment:

#### Dev Environment

```bash
cd infra/kubernetes/overlays/dev
kubectl apply -k .
```

#### Staging Environment

```bash
cd infra/kubernetes/overlays/staging
kubectl apply -k .
```

#### Production Environment

```bash
cd infra/kubernetes/overlays/prod
kubectl apply -k .
```

### Step 8: Verify SSL Certificates

Check that certificates are being issued for each environment:

```bash
# Dev
kubectl get certificate -n mys-dev
kubectl describe certificate mystira-publisher-tls -n mys-dev

# Staging
kubectl get certificate -n mys-staging
kubectl describe certificate mystira-publisher-tls -n mys-staging

# Production
kubectl get certificate -n mys-prod
kubectl describe certificate mystira-publisher-tls -n mys-prod
```

Certificates should move to "Ready" state within a few minutes.

**Note:** Dev and Staging use Let's Encrypt Staging certificates (which browsers will warn about). Production uses Let's Encrypt Production certificates (trusted by browsers).

### Step 9: Verify DNS Resolution

Test DNS resolution for each environment:

```bash
# Dev
nslookup dev.publisher.mystira.app
nslookup dev.chain.mystira.app

# Staging
nslookup staging.publisher.mystira.app
nslookup staging.chain.mystira.app

# Production
nslookup publisher.mystira.app
nslookup chain.mystira.app
```

Each should resolve to the appropriate load balancer IP.

### Step 10: Test HTTPS Endpoints

Test the endpoints for each environment:

```bash
# Dev Publisher
curl https://dev.publisher.mystira.app/health

# Staging Publisher
curl https://staging.publisher.mystira.app/health

# Production Publisher
curl https://publisher.mystira.app/health
```

## Quick Access URLs

Once everything is set up, you can access the Publisher at:

- **Development:** https://dev.publisher.mystira.app
- **Staging:** https://staging.publisher.mystira.app
- **Production:** https://publisher.mystira.app

## Troubleshooting

### DNS Not Resolving

- Check nameserver configuration at registrar (production only)
- Wait for DNS propagation (up to 48 hours)
- Verify Azure DNS zone has correct records:
  ```bash
  # Check dev records
  az network dns record-set list --resource-group <dev-rg> --zone-name mystira.app | grep "dev\.publisher\|dev\.chain"
  
  # Check staging records
  az network dns record-set list --resource-group <staging-rg> --zone-name mystira.app | grep "staging\.publisher\|staging\.chain"
  
  # Check prod records
  az network dns record-set list --resource-group <prod-rg> --zone-name mystira.app | grep "^publisher\|^chain"
  ```

### Certificate Not Issuing

- Check cert-manager logs: `kubectl logs -n cert-manager -l app=cert-manager`
- Verify DNS is resolving correctly before requesting certificates
- Check HTTP-01 challenge: `kubectl describe challenge -n <namespace>`
- For dev/staging, staging certificates will show browser warnings (this is expected)

### Ingress Not Working

- Verify NGINX ingress controller is running: `kubectl get pods -n ingress-nginx`
- Check ingress controller logs: `kubectl logs -n ingress-nginx -l app.kubernetes.io/component=controller`
- Verify ingress resources: `kubectl get ingress -n <namespace>`
- Check if the correct host is configured: `kubectl get ingress -n <namespace> -o yaml`

### Wrong Environment URL

If you're seeing the wrong URL for an environment:

1. Check the kustomization overlay for that environment
2. Verify the ingress patch is applied correctly
3. Reapply the kustomization: `kubectl apply -k infra/kubernetes/overlays/<env>`

## CI/CD Integration

Update your CI/CD workflows to use the correct URLs:

```yaml
# Example: .github/workflows/infra-deploy.yml
- name: Test Deployment
  run: |
    if [ "${{ github.event.inputs.environment }}" == "prod" ]; then
      curl -f https://publisher.mystira.app/health
    else
      curl -f https://${{ github.event.inputs.environment }}.publisher.mystira.app/health
    fi
```

## Monitoring

Monitor your endpoints across all environments:

```bash
# Watch certificate status
watch kubectl get certificate -A

# Watch ingress status
watch kubectl get ingress -A

# Check all publisher endpoints
for env in dev staging prod; do
  if [ "$env" == "prod" ]; then
    url="https://publisher.mystira.app/health"
  else
    url="https://$env.publisher.mystira.app/health"
  fi
  echo "Testing $env: $url"
  curl -s -o /dev/null -w "%{http_code}\n" $url
done
```

## Security Notes

1. **Dev and Staging** use Let's Encrypt Staging certificates:
   - These will show browser security warnings
   - This is intentional to avoid rate limits during testing
   - Not trusted by browsers

2. **Production** uses Let's Encrypt Production certificates:
   - Fully trusted by all browsers
   - Auto-renews 30 days before expiry
   - Monitor certificate expiration

3. **Network Security:**
   - Consider IP whitelisting for dev/staging environments
   - Use Azure Application Gateway WAF for production
   - Enable rate limiting on ingress controller

## Next Steps

1. Set up monitoring and alerting for SSL certificate expiration
2. Configure rate limiting and DDoS protection on the ingress
3. Set up Azure Application Gateway with WAF rules (production)
4. Configure backup DNS provider for redundancy
5. Implement DNS-based health checks
6. Set up environment-specific monitoring dashboards
