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
# Detect sudo availability and set install paths
# -----------------------------------------------------------------------------
if command -v sudo &> /dev/null && sudo -n true 2>/dev/null; then
    # sudo is available and we can use it without password prompt
    SUDO="sudo"
    BIN_DIR="/usr/local/bin"
elif [ -w "/usr/local/bin" ]; then
    # No sudo needed, /usr/local/bin is writable
    SUDO=""
    BIN_DIR="/usr/local/bin"
else
    # Fallback to user-writable directory
    SUDO=""
    BIN_DIR="$HOME/.local/bin"
    mkdir -p "$BIN_DIR"
    # Ensure BIN_DIR is in PATH
    if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then
        export PATH="$BIN_DIR:$PATH"
        echo "export PATH=\"$BIN_DIR:\$PATH\"" >> "$HOME/.bashrc"
    fi
fi
echo "  Using BIN_DIR: $BIN_DIR (SUDO: ${SUDO:-none})"

# -----------------------------------------------------------------------------
# Helper function: verify SHA256 checksum
# -----------------------------------------------------------------------------
verify_checksum() {
    local file="$1"
    local expected_checksum="$2"
    local actual_checksum
    actual_checksum=$(sha256sum "$file" | awk '{print $1}')
    if [ "$actual_checksum" != "$expected_checksum" ]; then
        echo "ERROR: Checksum verification failed for $file"
        echo "  Expected: $expected_checksum"
        echo "  Actual:   $actual_checksum"
        rm -f "$file"
        return 1
    fi
    echo "  Checksum verified: $file"
    return 0
}

# -----------------------------------------------------------------------------
# .NET SDK 10.0
# -----------------------------------------------------------------------------
echo "Installing .NET SDK 10.0..."
if ! command -v dotnet &> /dev/null || [[ "$(dotnet --version 2>/dev/null | cut -d'.' -f1)" -lt 10 ]]; then
    wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
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
# .NET Restore (restore NuGet packages for the solution)
# -----------------------------------------------------------------------------
echo "Restoring .NET NuGet packages..."
if command -v dotnet &> /dev/null && [ -f "$CLAUDE_PROJECT_DIR/Mystira.sln" ]; then
    cd "$CLAUDE_PROJECT_DIR"
    dotnet restore Mystira.sln --verbosity minimal 2>/dev/null && echo "  .NET packages restored" || echo "  WARNING: dotnet restore failed (may need authentication for private feeds)"
fi

# -----------------------------------------------------------------------------
# Node.js 24 LTS + pnpm
# -----------------------------------------------------------------------------
NODE_VERSION="v24.0.0"
NODE_CHECKSUM="59b8af617dccd7f9f68cc8451b2aee1e86d6bd5cb92cd51dd6216a31b707efd7"
echo "Installing Node.js 24..."
if ! command -v node &> /dev/null || [[ "$(node --version | cut -d'.' -f1 | tr -d 'v')" -lt 24 ]]; then
    # Install Node.js via nvm or direct download with checksum verification
    if command -v nvm &> /dev/null; then
        nvm install 24
        nvm use 24
    else
        # Secure install: download pinned release with checksum verification
        NODE_ARCHIVE="node-${NODE_VERSION}-linux-x64.tar.xz"
        NODE_URL="https://nodejs.org/dist/${NODE_VERSION}/${NODE_ARCHIVE}"

        echo "  Downloading Node.js ${NODE_VERSION}..."
        wget -q "${NODE_URL}" -O "/tmp/${NODE_ARCHIVE}"

        # Verify checksum
        if ! verify_checksum "/tmp/${NODE_ARCHIVE}" "$NODE_CHECKSUM"; then
            echo "ERROR: Node.js checksum verification failed. Aborting."
            exit 1
        fi

        # Compute install prefix from BIN_DIR (strip trailing /bin)
        NODE_PREFIX="${BIN_DIR%/bin}"
        if [ "$NODE_PREFIX" = "$BIN_DIR" ]; then
            # BIN_DIR doesn't end with /bin, use parent directory
            NODE_PREFIX="$(dirname "$BIN_DIR")"
        fi

        # Check if NODE_PREFIX is writable when SUDO is not set
        if [ -z "$SUDO" ]; then
            mkdir -p "$NODE_PREFIX" 2>/dev/null || true
            if ! touch "$NODE_PREFIX/.write_test" 2>/dev/null; then
                echo "  NODE_PREFIX ($NODE_PREFIX) is not writable, falling back to \$HOME/.local"
                NODE_PREFIX="$HOME/.local"
                BIN_DIR="$HOME/.local/bin"
                mkdir -p "$NODE_PREFIX" "$BIN_DIR"
                # Ensure BIN_DIR is in PATH
                if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then
                    export PATH="$BIN_DIR:$PATH"
                fi
            else
                rm -f "$NODE_PREFIX/.write_test"
            fi
        fi

        # Extract to the computed prefix
        echo "  Installing Node.js to ${NODE_PREFIX}..."
        mkdir -p "$NODE_PREFIX"
        if [ -n "$SUDO" ]; then
            $SUDO tar -xJf "/tmp/${NODE_ARCHIVE}" -C "$NODE_PREFIX" --strip-components=1
        else
            tar -xJf "/tmp/${NODE_ARCHIVE}" -C "$NODE_PREFIX" --strip-components=1 || {
                echo "ERROR: Failed to extract Node.js. Aborting."
                exit 1
            }
        fi
        rm "/tmp/${NODE_ARCHIVE}"
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
# Azure CLI
# -----------------------------------------------------------------------------
echo "Installing Azure CLI..."
if ! command -v az &> /dev/null; then
    # Install via Microsoft's official script (handles dependencies)
    curl -sL https://aka.ms/InstallAzureCLIDeb | $SUDO bash 2>/dev/null || {
        # Fallback: install via pip if the script fails
        echo "  Falling back to pip installation..."
        pip3 install --user azure-cli 2>/dev/null || pip install --user azure-cli 2>/dev/null || {
            echo "WARNING: Could not install Azure CLI"
        }
    }
    if command -v az &> /dev/null; then
        echo "  Azure CLI installed: $(az version --query '\"azure-cli\"' -o tsv 2>/dev/null || az --version | head -1)"
    fi
else
    echo "  Azure CLI already installed: $(az version --query '\"azure-cli\"' -o tsv 2>/dev/null || az --version | head -1)"
fi

# -----------------------------------------------------------------------------
# Terraform (pinned version with checksum verification)
# -----------------------------------------------------------------------------
TF_VERSION="1.10.3"
TF_CHECKSUM="ea3020db6b53c25a4a84e40cdc36c1a86df26967d718219ab4c71b44435da81e"
echo "Installing Terraform ${TF_VERSION}..."
if ! command -v terraform &> /dev/null || [[ "$(terraform version -json 2>/dev/null | jq -r '.terraform_version' 2>/dev/null)" != "$TF_VERSION" ]]; then
    TF_ARCHIVE="terraform_${TF_VERSION}_linux_amd64.zip"
    TF_URL="https://releases.hashicorp.com/terraform/${TF_VERSION}/${TF_ARCHIVE}"

    echo "  Downloading Terraform ${TF_VERSION}..."
    wget -q "${TF_URL}" -O "/tmp/${TF_ARCHIVE}"

    # Verify checksum
    if ! verify_checksum "/tmp/${TF_ARCHIVE}" "$TF_CHECKSUM"; then
        echo "ERROR: Terraform checksum verification failed. Aborting."
        exit 1
    fi

    $SUDO unzip -o -q "/tmp/${TF_ARCHIVE}" -d "$BIN_DIR/"
    rm "/tmp/${TF_ARCHIVE}"
    echo "  Terraform installed: $(terraform version | head -1)"
else
    echo "  Terraform already installed: $(terraform version | head -1)"
fi

# -----------------------------------------------------------------------------
# Terragrunt (pinned version with checksum verification)
# -----------------------------------------------------------------------------
TG_VERSION="0.69.1"
TG_CHECKSUM="eb0e3558bb453241301126a15a9eeee3592817d8013ddd44793aeac168da9ad1"
echo "Installing Terragrunt ${TG_VERSION}..."
if ! command -v terragrunt &> /dev/null; then
    TG_BINARY="terragrunt_linux_amd64"
    TG_URL="https://github.com/gruntwork-io/terragrunt/releases/download/v${TG_VERSION}/${TG_BINARY}"

    echo "  Downloading Terragrunt ${TG_VERSION}..."
    wget -q "${TG_URL}" -O "/tmp/${TG_BINARY}"

    # Verify checksum
    if ! verify_checksum "/tmp/${TG_BINARY}" "$TG_CHECKSUM"; then
        echo "ERROR: Terragrunt checksum verification failed. Aborting."
        exit 1
    fi

    chmod +x "/tmp/${TG_BINARY}"
    $SUDO mv "/tmp/${TG_BINARY}" "$BIN_DIR/terragrunt"
    echo "  Terragrunt installed: $(terragrunt --version | head -1)"
else
    echo "  Terragrunt already installed: $(terragrunt --version | head -1)"
fi

# -----------------------------------------------------------------------------
# TFLint (pinned version with checksum verification)
# -----------------------------------------------------------------------------
TFLINT_VERSION="v0.60.0"
TFLINT_CHECKSUM="3476ceedcf0c4f9f2bed35e92988e1411bec2caa543c9387bffaa720df9efaf7"
echo "Installing TFLint ${TFLINT_VERSION}..."
if ! command -v tflint &> /dev/null; then
    TFLINT_ARCHIVE="tflint_linux_amd64.zip"
    TFLINT_URL="https://github.com/terraform-linters/tflint/releases/download/${TFLINT_VERSION}/${TFLINT_ARCHIVE}"

    echo "  Downloading TFLint ${TFLINT_VERSION}..."
    wget -q "${TFLINT_URL}" -O "/tmp/${TFLINT_ARCHIVE}"

    # Verify checksum
    if ! verify_checksum "/tmp/${TFLINT_ARCHIVE}" "$TFLINT_CHECKSUM"; then
        echo "ERROR: TFLint checksum verification failed. Aborting."
        exit 1
    fi

    # Extract and install
    unzip -o -q "/tmp/${TFLINT_ARCHIVE}" -d /tmp/tflint_extracted
    chmod +x /tmp/tflint_extracted/tflint
    $SUDO mv /tmp/tflint_extracted/tflint "$BIN_DIR/"
    rm -rf "/tmp/${TFLINT_ARCHIVE}" /tmp/tflint_extracted

    echo "  TFLint installed: $(tflint --version | head -1)"
else
    echo "  TFLint already installed: $(tflint --version | head -1)"
fi

# -----------------------------------------------------------------------------
# Kustomize (pinned version with checksum verification)
# -----------------------------------------------------------------------------
KUSTOMIZE_VERSION="v5.8.0"
KUSTOMIZE_CHECKSUM="4dfa8307358dd9284aa4d2b1d5596766a65b93433e8fa3f9f74498941f01c5ef"
echo "Installing Kustomize ${KUSTOMIZE_VERSION}..."
if ! command -v kustomize &> /dev/null; then
    KUSTOMIZE_ARCHIVE="kustomize_${KUSTOMIZE_VERSION}_linux_amd64.tar.gz"
    KUSTOMIZE_URL="https://github.com/kubernetes-sigs/kustomize/releases/download/kustomize%2F${KUSTOMIZE_VERSION}/${KUSTOMIZE_ARCHIVE}"

    echo "  Downloading Kustomize ${KUSTOMIZE_VERSION}..."
    wget -q "${KUSTOMIZE_URL}" -O "/tmp/${KUSTOMIZE_ARCHIVE}"

    # Verify checksum
    if ! verify_checksum "/tmp/${KUSTOMIZE_ARCHIVE}" "$KUSTOMIZE_CHECKSUM"; then
        echo "ERROR: Kustomize checksum verification failed. Aborting."
        exit 1
    fi

    # Extract and install
    tar -xzf "/tmp/${KUSTOMIZE_ARCHIVE}" -C /tmp
    chmod +x /tmp/kustomize
    $SUDO mv /tmp/kustomize "$BIN_DIR/"
    rm -f "/tmp/${KUSTOMIZE_ARCHIVE}"

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
    $SUDO mv kubectl "$BIN_DIR/"
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
    $SUDO mv /tmp/hadolint "$BIN_DIR/hadolint"
    echo "  Hadolint installed: $(hadolint --version)"
else
    echo "  Hadolint already installed: $(hadolint --version)"
fi

# -----------------------------------------------------------------------------
# jq (JSON processor - useful for many scripts)
# -----------------------------------------------------------------------------
echo "Installing jq..."
if ! command -v jq &> /dev/null; then
    $SUDO apt-get update -qq && $SUDO apt-get install -y -qq jq 2>/dev/null || {
        wget -q "https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-linux-amd64" -O /tmp/jq
        chmod +x /tmp/jq
        $SUDO mv /tmp/jq "$BIN_DIR/jq"
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
    $SUDO mv /tmp/yq "$BIN_DIR/yq"
    echo "  yq installed: $(yq --version)"
else
    echo "  yq already installed: $(yq --version)"
fi

echo ""
echo "=== Tool Installation Complete ==="
echo ""

exit 0
