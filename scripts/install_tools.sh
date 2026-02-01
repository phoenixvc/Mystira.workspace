#!/bin/bash
# =============================================================================
# Mystira Development Tools Installer
# =============================================================================
# This script installs all tools required for local development and CI/CD
# Used by Claude Code SessionStart hook for web sessions
# =============================================================================

set -e

echo "=== Installing Mystira Development Tools ==="

# -----------------------------------------------------------------------------
# .NET SDK 9.0
# -----------------------------------------------------------------------------
echo "Installing .NET SDK 9.0..."
if ! command -v dotnet &> /dev/null; then
    wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet"
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$HOME/.dotnet:$PATH"
    echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.bashrc"
    echo 'export PATH="$HOME/.dotnet:$PATH"' >> "$HOME/.bashrc"
    rm /tmp/dotnet-install.sh
    echo "  .NET SDK installed: $(dotnet --version)"
else
    echo "  .NET SDK already installed: $(dotnet --version)"
fi

# -----------------------------------------------------------------------------
# Node.js 24 LTS + pnpm
# -----------------------------------------------------------------------------
echo "Installing Node.js 24..."
if ! command -v node &> /dev/null || [[ "$(node --version | cut -d'.' -f1 | tr -d 'v')" -lt 24 ]]; then
    # Install Node.js via nvm or direct download
    if command -v nvm &> /dev/null; then
        nvm install 24
        nvm use 24
    else
        # Direct install via NodeSource
        curl -fsSL https://deb.nodesource.com/setup_24.x | sudo -E bash - 2>/dev/null || true
        sudo apt-get install -y nodejs 2>/dev/null || {
            # Fallback: download directly
            NODE_VERSION="v24.0.0"
            wget -q "https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.xz" -O /tmp/node.tar.xz
            sudo tar -xJf /tmp/node.tar.xz -C /usr/local --strip-components=1
            rm /tmp/node.tar.xz
        }
    fi
    echo "  Node.js installed: $(node --version)"
else
    echo "  Node.js already installed: $(node --version)"
fi

echo "Installing pnpm..."
if ! command -v pnpm &> /dev/null; then
    npm install -g pnpm
    echo "  pnpm installed: $(pnpm --version)"
else
    echo "  pnpm already installed: $(pnpm --version)"
fi

# -----------------------------------------------------------------------------
# Terraform 1.10.3
# -----------------------------------------------------------------------------
echo "Installing Terraform 1.10.3..."
TF_VERSION="1.10.3"
if ! command -v terraform &> /dev/null || [[ "$(terraform version -json 2>/dev/null | jq -r '.terraform_version' 2>/dev/null)" != "$TF_VERSION" ]]; then
    wget -q "https://releases.hashicorp.com/terraform/${TF_VERSION}/terraform_${TF_VERSION}_linux_amd64.zip" -O /tmp/terraform.zip
    sudo unzip -o /tmp/terraform.zip -d /usr/local/bin/
    rm /tmp/terraform.zip
    echo "  Terraform installed: $(terraform version | head -1)"
else
    echo "  Terraform already installed: $(terraform version | head -1)"
fi

# -----------------------------------------------------------------------------
# Terragrunt 0.69.1
# -----------------------------------------------------------------------------
echo "Installing Terragrunt 0.69.1..."
TG_VERSION="0.69.1"
if ! command -v terragrunt &> /dev/null; then
    wget -q "https://github.com/gruntwork-io/terragrunt/releases/download/v${TG_VERSION}/terragrunt_linux_amd64" -O /tmp/terragrunt
    chmod +x /tmp/terragrunt
    sudo mv /tmp/terragrunt /usr/local/bin/
    echo "  Terragrunt installed: $(terragrunt --version | head -1)"
else
    echo "  Terragrunt already installed: $(terragrunt --version | head -1)"
fi

# -----------------------------------------------------------------------------
# TFLint (latest)
# -----------------------------------------------------------------------------
echo "Installing TFLint..."
if ! command -v tflint &> /dev/null; then
    curl -s https://raw.githubusercontent.com/terraform-linters/tflint/master/install_linux.sh | bash
    echo "  TFLint installed: $(tflint --version | head -1)"
else
    echo "  TFLint already installed: $(tflint --version | head -1)"
fi

# -----------------------------------------------------------------------------
# Kustomize (latest)
# -----------------------------------------------------------------------------
echo "Installing Kustomize..."
if ! command -v kustomize &> /dev/null; then
    curl -s "https://raw.githubusercontent.com/kubernetes-sigs/kustomize/master/hack/install_kustomize.sh" | bash
    sudo mv kustomize /usr/local/bin/
    echo "  Kustomize installed: $(kustomize version)"
else
    echo "  Kustomize already installed: $(kustomize version)"
fi

# -----------------------------------------------------------------------------
# kubectl (latest stable)
# -----------------------------------------------------------------------------
echo "Installing kubectl..."
if ! command -v kubectl &> /dev/null; then
    curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
    chmod +x kubectl
    sudo mv kubectl /usr/local/bin/
    echo "  kubectl installed: $(kubectl version --client --short 2>/dev/null || kubectl version --client)"
else
    echo "  kubectl already installed: $(kubectl version --client --short 2>/dev/null || kubectl version --client)"
fi

# -----------------------------------------------------------------------------
# Python 3.12 + pip-audit
# -----------------------------------------------------------------------------
echo "Installing Python tools..."
if command -v python3 &> /dev/null; then
    pip3 install --user pip-audit 2>/dev/null || pip install --user pip-audit 2>/dev/null || true
    echo "  Python: $(python3 --version)"
fi

# -----------------------------------------------------------------------------
# ESLint & Prettier (via pnpm - project dependencies)
# -----------------------------------------------------------------------------
echo "Installing Node.js project dependencies (includes ESLint & Prettier)..."
if [ -f "$CLAUDE_PROJECT_DIR/package.json" ]; then
    cd "$CLAUDE_PROJECT_DIR"
    pnpm install --frozen-lockfile 2>/dev/null || pnpm install 2>/dev/null || true
    echo "  Node.js dependencies installed"
fi

# -----------------------------------------------------------------------------
# Hadolint (Dockerfile linter)
# -----------------------------------------------------------------------------
echo "Installing Hadolint..."
HADOLINT_VERSION="v2.12.0"
if ! command -v hadolint &> /dev/null; then
    wget -q "https://github.com/hadolint/hadolint/releases/download/${HADOLINT_VERSION}/hadolint-Linux-x86_64" -O /tmp/hadolint
    chmod +x /tmp/hadolint
    sudo mv /tmp/hadolint /usr/local/bin/hadolint
    echo "  Hadolint installed: $(hadolint --version)"
else
    echo "  Hadolint already installed: $(hadolint --version)"
fi

# -----------------------------------------------------------------------------
# jq (JSON processor - useful for many scripts)
# -----------------------------------------------------------------------------
echo "Installing jq..."
if ! command -v jq &> /dev/null; then
    sudo apt-get update -qq && sudo apt-get install -y -qq jq 2>/dev/null || {
        wget -q "https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-linux-amd64" -O /tmp/jq
        chmod +x /tmp/jq
        sudo mv /tmp/jq /usr/local/bin/jq
    }
    echo "  jq installed: $(jq --version)"
else
    echo "  jq already installed: $(jq --version)"
fi

# -----------------------------------------------------------------------------
# yq (YAML processor)
# -----------------------------------------------------------------------------
echo "Installing yq..."
if ! command -v yq &> /dev/null; then
    wget -q "https://github.com/mikefarah/yq/releases/download/v4.44.1/yq_linux_amd64" -O /tmp/yq
    chmod +x /tmp/yq
    sudo mv /tmp/yq /usr/local/bin/yq
    echo "  yq installed: $(yq --version)"
else
    echo "  yq already installed: $(yq --version)"
fi

echo ""
echo "=== Tool Installation Complete ==="
echo ""

exit 0
