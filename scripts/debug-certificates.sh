#!/bin/bash

# Certificate Debugging Script for Dev Environment
# This script checks all components related to SSL/TLS certificate configuration

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
NAMESPACE="mys-dev"
DOMAINS=(
    "dev.publisher.mystira.app"
    "dev.chain.mystira.app"
    "dev.story-generator.mystira.app"
)
CERT_SECRETS=(
    "mystira-publisher-tls-dev"
    "mystira-chain-tls-dev"
    "mystira-story-generator-tls-dev"
)
INGRESSES=(
    "mystira-publisher-ingress"
    "mystira-chain-ingress"
    "mystira-story-generator-ingress"
)

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Certificate Debugging Script - Dev Env${NC}"
echo -e "${BLUE}========================================${NC}\n"

# Function to print status
print_status() {
    local status=$1
    local message=$2
    if [ "$status" = "OK" ]; then
        echo -e "${GREEN}✓${NC} $message"
    elif [ "$status" = "WARN" ]; then
        echo -e "${YELLOW}⚠${NC} $message"
    else
        echo -e "${RED}✗${NC} $message"
    fi
}

# Function to print section header
section_header() {
    echo -e "\n${BLUE}=== $1 ===${NC}"
}

# Check if kubectl is configured
section_header "1. Kubernetes Connection"
if ! kubectl cluster-info &> /dev/null; then
    print_status "ERROR" "kubectl is not configured or cannot connect to cluster"
    echo -e "${YELLOW}Please configure kubectl to connect to your AKS cluster:${NC}"
    echo "  az aks get-credentials --resource-group mys-dev-core-rg-san --name mys-dev-core-aks-san"
    exit 1
fi
print_status "OK" "Successfully connected to Kubernetes cluster"
kubectl cluster-info | head -1

# Check namespace
section_header "2. Namespace Check"
if kubectl get namespace "$NAMESPACE" &> /dev/null; then
    print_status "OK" "Namespace '$NAMESPACE' exists"
else
    print_status "ERROR" "Namespace '$NAMESPACE' does not exist"
    exit 1
fi

# Check DNS resolution
section_header "3. DNS Resolution"
for domain in "${DOMAINS[@]}"; do
    if host "$domain" &> /dev/null; then
        ip=$(host "$domain" | grep "has address" | awk '{print $4}' | head -1)
        if [ -n "$ip" ]; then
            print_status "OK" "$domain resolves to $ip"
        else
            print_status "WARN" "$domain DNS record exists but no A record found"
        fi
    else
        print_status "ERROR" "$domain does not resolve"
    fi
done

# Check NGINX Ingress Controller
section_header "4. NGINX Ingress Controller"
if kubectl get pods -n ingress-nginx &> /dev/null; then
    nginx_ready=$(kubectl get pods -n ingress-nginx -l app.kubernetes.io/component=controller -o jsonpath='{.items[0].status.conditions[?(@.type=="Ready")].status}' 2>/dev/null)
    if [ "$nginx_ready" = "True" ]; then
        print_status "OK" "NGINX Ingress Controller is running and ready"
        kubectl get pods -n ingress-nginx -l app.kubernetes.io/component=controller
    else
        print_status "ERROR" "NGINX Ingress Controller is not ready"
        kubectl get pods -n ingress-nginx
    fi
else
    print_status "ERROR" "NGINX Ingress Controller namespace not found"
fi

# Check cert-manager
section_header "5. cert-manager Status"
if kubectl get pods -n cert-manager &> /dev/null; then
    certmgr_ready=$(kubectl get pods -n cert-manager -l app=cert-manager -o jsonpath='{.items[0].status.conditions[?(@.type=="Ready")].status}' 2>/dev/null)
    if [ "$certmgr_ready" = "True" ]; then
        print_status "OK" "cert-manager is running and ready"
        kubectl get pods -n cert-manager
    else
        print_status "ERROR" "cert-manager is not ready"
        kubectl get pods -n cert-manager
    fi
else
    print_status "ERROR" "cert-manager namespace not found"
fi

# Check ClusterIssuers
section_header "6. Certificate Issuers"
if kubectl get clusterissuer letsencrypt-staging &> /dev/null; then
    issuer_ready=$(kubectl get clusterissuer letsencrypt-staging -o jsonpath='{.status.conditions[?(@.type=="Ready")].status}' 2>/dev/null)
    if [ "$issuer_ready" = "True" ]; then
        print_status "OK" "letsencrypt-staging ClusterIssuer is ready"
    else
        print_status "WARN" "letsencrypt-staging ClusterIssuer is not ready"
    fi
else
    print_status "ERROR" "letsencrypt-staging ClusterIssuer not found"
fi

if kubectl get clusterissuer letsencrypt-prod &> /dev/null; then
    issuer_ready=$(kubectl get clusterissuer letsencrypt-prod -o jsonpath='{.status.conditions[?(@.type=="Ready")].status}' 2>/dev/null)
    if [ "$issuer_ready" = "True" ]; then
        print_status "OK" "letsencrypt-prod ClusterIssuer is ready"
    else
        print_status "WARN" "letsencrypt-prod ClusterIssuer is not ready"
    fi
else
    print_status "ERROR" "letsencrypt-prod ClusterIssuer not found"
fi

# Check Ingresses
section_header "7. Ingress Resources"
for i in "${!INGRESSES[@]}"; do
    ingress="${INGRESSES[$i]}"
    domain="${DOMAINS[$i]}"

    echo -e "\n${YELLOW}Checking: $ingress${NC}"

    if kubectl get ingress "$ingress" -n "$NAMESPACE" &> /dev/null; then
        print_status "OK" "Ingress '$ingress' exists"

        # Check host configuration
        configured_host=$(kubectl get ingress "$ingress" -n "$NAMESPACE" -o jsonpath='{.spec.rules[0].host}')
        if [ "$configured_host" = "$domain" ]; then
            print_status "OK" "Host configured correctly: $configured_host"
        else
            print_status "ERROR" "Host mismatch! Expected: $domain, Got: $configured_host"
        fi

        # Check TLS configuration
        tls_host=$(kubectl get ingress "$ingress" -n "$NAMESPACE" -o jsonpath='{.spec.tls[0].hosts[0]}')
        tls_secret=$(kubectl get ingress "$ingress" -n "$NAMESPACE" -o jsonpath='{.spec.tls[0].secretName}')
        if [ "$tls_host" = "$domain" ]; then
            print_status "OK" "TLS host configured correctly: $tls_host"
        else
            print_status "ERROR" "TLS host mismatch! Expected: $domain, Got: $tls_host"
        fi
        print_status "OK" "TLS secret: $tls_secret"

        # Check cert-manager annotation
        cert_issuer=$(kubectl get ingress "$ingress" -n "$NAMESPACE" -o jsonpath='{.metadata.annotations.cert-manager\.io/cluster-issuer}')
        if [ "$cert_issuer" = "letsencrypt-staging" ]; then
            print_status "OK" "Using correct issuer: $cert_issuer"
        elif [ "$cert_issuer" = "letsencrypt-prod" ]; then
            print_status "WARN" "Using production issuer (should be staging for dev): $cert_issuer"
        else
            print_status "ERROR" "No cert-manager issuer annotation found or invalid: '$cert_issuer'"
        fi

        # Check ingress IP
        ingress_ip=$(kubectl get ingress "$ingress" -n "$NAMESPACE" -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
        if [ -n "$ingress_ip" ]; then
            print_status "OK" "Ingress has external IP: $ingress_ip"
        else
            print_status "WARN" "Ingress does not have an external IP yet"
        fi
    else
        print_status "ERROR" "Ingress '$ingress' not found in namespace '$NAMESPACE'"
    fi
done

# Check Certificate Resources
section_header "8. Certificate Resources"
if kubectl get certificates -n "$NAMESPACE" &> /dev/null; then
    echo -e "\n${YELLOW}All certificates in namespace:${NC}"
    kubectl get certificates -n "$NAMESPACE"

    for i in "${!CERT_SECRETS[@]}"; do
        cert_name="${CERT_SECRETS[$i]}"
        echo -e "\n${YELLOW}Checking certificate: $cert_name${NC}"

        if kubectl get certificate "$cert_name" -n "$NAMESPACE" &> /dev/null; then
            cert_ready=$(kubectl get certificate "$cert_name" -n "$NAMESPACE" -o jsonpath='{.status.conditions[?(@.type=="Ready")].status}' 2>/dev/null)
            if [ "$cert_ready" = "True" ]; then
                print_status "OK" "Certificate '$cert_name' is ready"
            else
                print_status "ERROR" "Certificate '$cert_name' is NOT ready"
                echo -e "${YELLOW}Certificate status:${NC}"
                kubectl get certificate "$cert_name" -n "$NAMESPACE" -o jsonpath='{.status.conditions[?(@.type=="Ready")]}' | jq '.'
            fi

            # Show certificate details
            echo -e "${YELLOW}Certificate details:${NC}"
            kubectl describe certificate "$cert_name" -n "$NAMESPACE" | grep -A 5 "Status:"
        else
            print_status "WARN" "Certificate resource '$cert_name' not found (will be created by cert-manager)"
        fi
    done
else
    print_status "WARN" "No certificate resources found in namespace '$NAMESPACE'"
fi

# Check TLS Secrets
section_header "9. TLS Secrets"
for secret in "${CERT_SECRETS[@]}"; do
    echo -e "\n${YELLOW}Checking secret: $secret${NC}"

    if kubectl get secret "$secret" -n "$NAMESPACE" &> /dev/null; then
        print_status "OK" "TLS secret '$secret' exists"

        # Check certificate expiration
        cert_data=$(kubectl get secret "$secret" -n "$NAMESPACE" -o jsonpath='{.data.tls\.crt}' | base64 -d)
        if [ -n "$cert_data" ]; then
            expiry=$(echo "$cert_data" | openssl x509 -noout -enddate 2>/dev/null | cut -d= -f2)
            if [ -n "$expiry" ]; then
                print_status "OK" "Certificate expires: $expiry"

                # Check issuer
                issuer=$(echo "$cert_data" | openssl x509 -noout -issuer 2>/dev/null)
                if echo "$issuer" | grep -q "Staging"; then
                    print_status "OK" "Issued by: Let's Encrypt Staging (correct for dev)"
                elif echo "$issuer" | grep -q "Let's Encrypt"; then
                    print_status "WARN" "Issued by: Let's Encrypt Production (should be staging for dev)"
                else
                    print_status "WARN" "Issuer: $issuer"
                fi

                # Check subject
                subject=$(echo "$cert_data" | openssl x509 -noout -subject 2>/dev/null)
                print_status "OK" "Subject: $subject"
            fi
        else
            print_status "ERROR" "TLS secret exists but has no certificate data"
        fi
    else
        print_status "WARN" "TLS secret '$secret' not found (will be created by cert-manager)"
    fi
done

# Check cert-manager logs for errors
section_header "10. Recent cert-manager Logs"
echo -e "${YELLOW}Last 20 lines from cert-manager (errors only):${NC}"
kubectl logs -n cert-manager deployment/cert-manager --tail=50 | grep -i error || echo "No errors found in recent logs"

# Check Certificate Requests
section_header "11. Certificate Requests"
if kubectl get certificaterequests -n "$NAMESPACE" &> /dev/null; then
    echo -e "${YELLOW}Recent certificate requests:${NC}"
    kubectl get certificaterequests -n "$NAMESPACE" --sort-by=.metadata.creationTimestamp | tail -10
else
    print_status "WARN" "No certificate requests found"
fi

# Test HTTPS connection
section_header "12. HTTPS Connection Test"
for domain in "${DOMAINS[@]}"; do
    echo -e "\n${YELLOW}Testing HTTPS for: $domain${NC}"

    # Test with curl
    if curl -sI --max-time 5 "https://$domain" &> /dev/null; then
        print_status "OK" "HTTPS connection successful"

        # Get certificate info
        cert_info=$(echo | openssl s_client -servername "$domain" -connect "$domain:443" 2>/dev/null | openssl x509 -noout -subject -issuer -dates 2>/dev/null)
        if [ -n "$cert_info" ]; then
            echo -e "${YELLOW}Certificate info:${NC}"
            echo "$cert_info"
        fi
    else
        print_status "ERROR" "HTTPS connection failed or timed out"

        # Try to get more details
        echo -e "${YELLOW}Attempting detailed connection test:${NC}"
        timeout 5 openssl s_client -servername "$domain" -connect "$domain:443" </dev/null 2>&1 | head -20 || true
    fi
done

# Summary
section_header "Summary & Next Steps"
echo -e "\n${BLUE}If you see any errors above:${NC}"
echo -e "1. ${YELLOW}Certificate not ready?${NC} Wait 5-10 minutes for cert-manager to issue certificates"
echo -e "2. ${YELLOW}Wrong issuer (prod instead of staging)?${NC} Redeploy the Kubernetes configuration"
echo -e "3. ${YELLOW}DNS not resolving?${NC} Check Azure DNS records in the mystira.app zone"
echo -e "4. ${YELLOW}Ingress not found?${NC} Deploy the Kubernetes manifests first"
echo -e "5. ${YELLOW}HTTPS connection fails?${NC} Check browser cache or wait for DNS propagation"
echo -e "\n${BLUE}Useful commands:${NC}"
echo -e "  ${GREEN}Force certificate renewal:${NC}"
echo -e "    kubectl delete certificate <cert-name> -n $NAMESPACE"
echo -e "    kubectl delete secret <secret-name> -n $NAMESPACE"
echo -e "  ${GREEN}Check cert-manager logs:${NC}"
echo -e "    kubectl logs -n cert-manager deployment/cert-manager -f"
echo -e "  ${GREEN}Describe certificate:${NC}"
echo -e "    kubectl describe certificate <cert-name> -n $NAMESPACE"
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}Debug script completed!${NC}"
echo -e "${BLUE}========================================${NC}\n"
