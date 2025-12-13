# DNS and Ingress Setup Guide

This guide explains how to set up DNS and ingress for the Mystira infrastructure with the domain names:

- `publisher.mystira.app`
- `chain.mystira.app`

## Prerequisites

1. Access to Azure subscription with appropriate permissions
2. Control of the `mystira.app` domain at your domain registrar
3. kubectl configured with access to your AKS clusters
4. Terraform >= 1.5.0 installed

## Step 1: Deploy Infrastructure with Terraform

Deploy the infrastructure which includes the DNS zone:

```bash
cd infra/terraform/environments/prod
terraform init
terraform plan
terraform apply
```

This will create:

- Azure DNS Zone for `mystira.app`
- DNS A records for `publisher.mystira.app` and `chain.mystira.app` (when IPs are provided)
- All other infrastructure components

## Step 2: Get Name Servers

After Terraform deployment, get the Azure DNS name servers:

```bash
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

## Step 3: Configure Domain Registrar

Log into your domain registrar where `mystira.app` is registered and:

1. Navigate to DNS settings for `mystira.app`
2. Replace the existing nameservers with the Azure DNS nameservers from Step 2
3. Save the changes

**Note:** DNS propagation can take up to 48 hours but typically completes within a few hours.

## Step 4: Install NGINX Ingress Controller

If not already installed in your AKS cluster:

```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

helm install nginx-ingress ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz
```

Wait for the load balancer to be provisioned:

```bash
kubectl get svc -n ingress-nginx nginx-ingress-ingress-nginx-controller -w
```

Note the external IP address assigned.

## Step 5: Install cert-manager

Install cert-manager for automated SSL certificate management:

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

Wait for cert-manager to be ready:

```bash
kubectl wait --for=condition=ready pod -l app.kubernetes.io/instance=cert-manager -n cert-manager --timeout=300s
```

## Step 6: Update DNS Records with Load Balancer IP

Once you have the load balancer external IP, update the Terraform configuration:

```bash
cd infra/terraform/environments/prod

# Edit main.tf and update the DNS module configuration:
# module "dns" {
#   source = "../../modules/dns"
#   ...
#   publisher_ip = "<LOAD_BALANCER_IP>"
#   chain_ip     = "<LOAD_BALANCER_IP>"
# }

terraform plan
terraform apply
```

Alternatively, manually add the A records in Azure DNS:

```bash
az network dns record-set a add-record \
  --resource-group mystira-prod-rg \
  --zone-name mystira.app \
  --record-set-name publisher \
  --ipv4-address <LOAD_BALANCER_IP>

az network dns record-set a add-record \
  --resource-group mystira-prod-rg \
  --zone-name mystira.app \
  --record-set-name chain \
  --ipv4-address <LOAD_BALANCER_IP>
```

## Step 7: Deploy Kubernetes Resources

Deploy the Kubernetes resources including ingress and cert-manager configuration:

```bash
cd infra/kubernetes/overlays/prod
kubectl apply -k .
```

This will deploy:

- Chain StatefulSet with RPC/WebSocket services
- Publisher Deployment with HTTP API
- Ingress resources for both services
- ClusterIssuer for Let's Encrypt SSL certificates

## Step 8: Verify SSL Certificates

Check that certificates are being issued:

```bash
# Check certificate requests
kubectl get certificaterequest -n mystira-prod

# Check certificates
kubectl get certificate -n mystira-prod

# Check certificate details
kubectl describe certificate mystira-publisher-tls -n mystira-prod
kubectl describe certificate mystira-chain-tls -n mystira-prod
```

Certificates should move to "Ready" state within a few minutes.

## Step 9: Verify DNS Resolution

Test DNS resolution:

```bash
nslookup publisher.mystira.app
nslookup chain.mystira.app
```

Both should resolve to your load balancer IP.

## Step 10: Test HTTPS Endpoints

Test the endpoints:

```bash
# Publisher service
curl https://publisher.mystira.app/health

# Chain RPC endpoint
curl -X POST https://chain.mystira.app \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"eth_blockNumber","params":[],"id":1}'
```

## Troubleshooting

### DNS Not Resolving

- Check nameserver configuration at registrar
- Wait for DNS propagation (up to 48 hours)
- Verify Azure DNS zone has correct records: `az network dns record-set list --resource-group mystira-prod-rg --zone-name mystira.app`

### Certificate Not Issuing

- Check cert-manager logs: `kubectl logs -n cert-manager -l app=cert-manager`
- Verify DNS is resolving correctly before requesting certificates
- Check HTTP-01 challenge: `kubectl describe challenge -n mystira-prod`

### Ingress Not Working

- Verify NGINX ingress controller is running: `kubectl get pods -n ingress-nginx`
- Check ingress controller logs: `kubectl logs -n ingress-nginx -l app.kubernetes.io/component=controller`
- Verify ingress resources: `kubectl get ingress -n mystira-prod`

### Load Balancer IP Not Assigned

- Check service status: `kubectl describe svc -n ingress-nginx nginx-ingress-ingress-nginx-controller`
- Verify Azure subscription has available public IPs
- Check Azure resource quotas

## Monitoring

Monitor your endpoints:

```bash
# Watch certificate status
kubectl get certificate -n mystira-prod -w

# Watch ingress status
kubectl get ingress -n mystira-prod -w

# Check ingress controller logs
kubectl logs -n ingress-nginx -l app.kubernetes.io/component=controller -f
```

## Environment-Specific Notes

### Development Environment

For dev environment, you may want to use staging Let's Encrypt:

- The ClusterIssuer includes both `letsencrypt-prod` and `letsencrypt-staging`
- Change ingress annotation to: `cert-manager.io/cluster-issuer: "letsencrypt-staging"`

### Production Environment

For production:

- Ensure DNS is fully propagated before deploying
- Use production Let's Encrypt issuer (already configured)
- Monitor certificate expiration (auto-renewal happens 30 days before expiry)
- Consider setting up monitoring alerts for certificate expiration

## Next Steps

1. Set up monitoring and alerting for SSL certificate expiration
2. Configure rate limiting and DDoS protection on the ingress
3. Set up Web Application Firewall (WAF) rules
4. Configure backup DNS provider for redundancy
5. Implement DNS-based health checks
