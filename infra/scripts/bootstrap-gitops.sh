#!/bin/bash

# Bootstrap script for Harness GitOps agent installation
# This is a one-time setup script - do not run in CI/CD

set -e

NAMESPACE="harness-gitops"
AGENT_YAML="infra/gitops/harness-agent.yaml"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "❌ kubectl is not installed or not in PATH"
    exit 1
fi

# Check if agent YAML exists
if [ ! -f "$REPO_ROOT/$AGENT_YAML" ]; then
    echo "❌ Agent YAML file not found: $REPO_ROOT/$AGENT_YAML"
    echo "   Please create the agent YAML file from the Harness platform"
    exit 1
fi

# Check if envsubst is available (for secret substitution)
if ! command -v envsubst &> /dev/null; then
    if [ -n "$HARNESS_AGENT_TOKEN" ]; then
        echo "❌ envsubst is not installed but HARNESS_AGENT_TOKEN is set."
        echo "   Cannot substitute secrets in $AGENT_YAML without envsubst."
        echo "   Install envsubst (part of gettext) or unset HARNESS_AGENT_TOKEN."
        exit 1
    fi
    echo "⚠️  envsubst not found. Secrets will not be substituted."
    echo "   Set HARNESS_AGENT_TOKEN if secret substitution is needed."
fi

echo "🚀 Bootstrapping Harness GitOps agent..."

# Create namespace if it doesn't exist
echo "📦 Creating namespace: $NAMESPACE"
kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

# Apply agent YAML with environment variable substitution
echo "📥 Installing Harness GitOps agent..."
cd "$REPO_ROOT"

if command -v envsubst &> /dev/null && [ -n "$HARNESS_AGENT_TOKEN" ]; then
    echo "   Using environment variable substitution for secrets"
    envsubst < "$AGENT_YAML" | kubectl apply -f - -n "$NAMESPACE"
else
    echo "   Applying YAML directly (no substitution)"
    kubectl apply -f "$AGENT_YAML" -n "$NAMESPACE"
fi

# Wait for agent to be ready
echo "⏳ Waiting for agent to be ready..."
if kubectl wait --for=condition=ready pod -l app=harness-gitops-agent -n "$NAMESPACE" --timeout=120s 2>/dev/null; then
    echo "✅ GitOps agent installed successfully!"
    echo ""
    echo "📊 Agent status:"
    kubectl get pods -n "$NAMESPACE" -l app=harness-gitops-agent
    echo ""
    echo "🔍 To view agent logs:"
    echo "   kubectl logs -l app=harness-gitops-agent -n $NAMESPACE"
else
    echo "⚠️  Agent pods may still be starting. Check status with:"
    echo "   kubectl get pods -n $NAMESPACE"
    echo "   kubectl logs -l app=harness-gitops-agent -n $NAMESPACE"
fi
