#!/bin/bash
# install_tools.sh - Setup development environment for Mystira.App
# Run this script to install all required tools for development

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  Mystira.App Development Setup${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Detect OS
OS="unknown"
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    OS="linux"
    if [ -f /etc/debian_version ]; then
        DISTRO="debian"
    elif [ -f /etc/redhat-release ]; then
        DISTRO="redhat"
    elif [ -f /etc/alpine-release ]; then
        DISTRO="alpine"
    fi
elif [[ "$OSTYPE" == "darwin"* ]]; then
    OS="macos"
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
    OS="windows"
fi

echo -e "${YELLOW}Detected OS: ${OS}${NC}"
echo ""

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to install .NET SDK
install_dotnet() {
    echo -e "${BLUE}[1/5] Installing .NET SDK 9.0...${NC}"

    if command_exists dotnet; then
        DOTNET_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
        if [[ "$DOTNET_VERSION" == 9.* ]]; then
            echo -e "${GREEN}  .NET SDK 9.x already installed (${DOTNET_VERSION})${NC}"
            return 0
        fi
    fi

    case $OS in
        linux)
            # Use Microsoft's install script
            curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
            chmod +x /tmp/dotnet-install.sh
            /tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet"

            # Add to PATH
            export DOTNET_ROOT="$HOME/.dotnet"
            export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"

            # Add to shell profile (only if not already present)
            if [ -f "$HOME/.bashrc" ] && ! grep -q '# Added by install_tools.sh: dotnet' "$HOME/.bashrc"; then
                echo '# Added by install_tools.sh: dotnet' >> "$HOME/.bashrc"
                echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.bashrc"
                echo 'export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"' >> "$HOME/.bashrc"
            fi
            if [ -f "$HOME/.zshrc" ] && ! grep -q '# Added by install_tools.sh: dotnet' "$HOME/.zshrc"; then
                echo '# Added by install_tools.sh: dotnet' >> "$HOME/.zshrc"
                echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.zshrc"
                echo 'export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"' >> "$HOME/.zshrc"
            fi
            ;;
        macos)
            if command_exists brew; then
                brew install dotnet@9
            else
                curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
                chmod +x /tmp/dotnet-install.sh
                /tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet"

                # Add to PATH
                export DOTNET_ROOT="$HOME/.dotnet"
                export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"

                # Add to shell profile (only if not already present)
                if [ -f "$HOME/.zshrc" ] && ! grep -q '# Added by install_tools.sh: dotnet' "$HOME/.zshrc"; then
                    echo '# Added by install_tools.sh: dotnet' >> "$HOME/.zshrc"
                    echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.zshrc"
                    echo 'export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"' >> "$HOME/.zshrc"
                fi
                if [ -f "$HOME/.bashrc" ] && ! grep -q '# Added by install_tools.sh: dotnet' "$HOME/.bashrc"; then
                    echo '# Added by install_tools.sh: dotnet' >> "$HOME/.bashrc"
                    echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.bashrc"
                    echo 'export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"' >> "$HOME/.bashrc"
                fi
            fi
            ;;
        windows)
            echo -e "${YELLOW}  On Windows, please install .NET SDK from:${NC}"
            echo -e "${YELLOW}  https://dotnet.microsoft.com/download/dotnet/9.0${NC}"
            return 0
            ;;
    esac

    echo -e "${GREEN}  .NET SDK installed successfully${NC}"
}

# Function to install Docker
install_docker() {
    echo -e "${BLUE}[2/5] Checking Docker...${NC}"

    if command_exists docker; then
        echo -e "${GREEN}  Docker already installed${NC}"
        return 0
    fi

    case $OS in
        linux)
            if [[ "$DISTRO" == "debian" ]]; then
                # Use official Docker apt repository (recommended over get.docker.com for reliability)
                # Remove old packages if they exist
                for pkg in docker.io docker-doc docker-compose podman-docker containerd runc; do
                    sudo apt-get remove -y $pkg 2>/dev/null || true
                done

                # Install prerequisites
                sudo apt-get update
                sudo apt-get install -y ca-certificates curl gnupg

                # Create keyrings directory
                sudo install -m 0755 -d /etc/apt/keyrings

                # Detect the OS for Docker repository (Ubuntu, Debian, or derivative)
                DOCKER_OS="ubuntu"
                CODENAME=""
                if [ -f /etc/os-release ]; then
                    . /etc/os-release
                    case "$ID" in
                        ubuntu)
                            DOCKER_OS="ubuntu"
                            CODENAME="${VERSION_CODENAME:-jammy}"
                            ;;
                        debian)
                            DOCKER_OS="debian"
                            CODENAME="${VERSION_CODENAME:-bookworm}"
                            ;;
                        linuxmint|neon|pop|elementary|zorin)
                            DOCKER_OS="ubuntu"
                            CODENAME="${UBUNTU_CODENAME:-${VERSION_CODENAME:-jammy}}"
                            ;;
                        *)
                            # Default to ubuntu for unknown Debian-based distros
                            DOCKER_OS="ubuntu"
                            CODENAME="${VERSION_CODENAME:-jammy}"
                            ;;
                    esac
                else
                    CODENAME="jammy"
                fi

                # Add Docker's official GPG key (--batch --yes to overwrite on re-runs)
                curl -fsSL "https://download.docker.com/linux/${DOCKER_OS}/gpg" | sudo gpg --batch --yes --dearmor -o /etc/apt/keyrings/docker.gpg
                sudo chmod a+r /etc/apt/keyrings/docker.gpg

                # Set up the repository (single line to avoid stray whitespace)
                local docker_arch
                docker_arch="$(dpkg --print-architecture)"
                printf 'deb [arch=%s signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/%s %s stable\n' \
                    "$docker_arch" "$DOCKER_OS" "$CODENAME" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

                # Install Docker Engine
                sudo apt-get update
                sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

                # Add user to docker group
                sudo usermod -aG docker "$USER"
                echo -e "${GREEN}  Docker installed successfully${NC}"
                echo -e "${YELLOW}  NOTE: You must log out and back in (or run 'newgrp docker') for docker group membership to take effect.${NC}"
                echo -e "${YELLOW}  After re-login, verify with: docker run hello-world${NC}"
            else
                echo -e "${YELLOW}  Please install Docker manually for your distribution${NC}"
            fi
            ;;
        macos)
            echo -e "${YELLOW}  Please install Docker Desktop from:${NC}"
            echo -e "${YELLOW}  https://www.docker.com/products/docker-desktop${NC}"
            ;;
        windows)
            echo -e "${YELLOW}  Please install Docker Desktop from:${NC}"
            echo -e "${YELLOW}  https://www.docker.com/products/docker-desktop${NC}"
            ;;
    esac
}

# Function to install Azure CLI
install_azure_cli() {
    echo -e "${BLUE}[3/5] Checking Azure CLI...${NC}"

    if command_exists az; then
        echo -e "${GREEN}  Azure CLI already installed${NC}"
        return 0
    fi

    case $OS in
        linux)
            if [[ "$DISTRO" == "debian" ]]; then
                curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
                echo -e "${GREEN}  Azure CLI installation complete${NC}"
            else
                echo -e "${YELLOW}  Please install Azure CLI manually for your distribution${NC}"
                return 0
            fi
            ;;
        macos)
            if command_exists brew; then
                brew install azure-cli
                echo -e "${GREEN}  Azure CLI installation complete${NC}"
            else
                echo -e "${YELLOW}  Please install Homebrew first, then run: brew install azure-cli${NC}"
                return 0
            fi
            ;;
        windows)
            echo -e "${YELLOW}  Please install Azure CLI from:${NC}"
            echo -e "${YELLOW}  https://aka.ms/installazurecliwindows${NC}"
            return 0
            ;;
    esac
}

# Function to install GitHub CLI
install_gh_cli() {
    echo -e "${BLUE}[4/5] Checking GitHub CLI...${NC}"

    if command_exists gh; then
        echo -e "${GREEN}  GitHub CLI already installed${NC}"
        return 0
    fi

    case $OS in
        linux)
            if [[ "$DISTRO" == "debian" ]]; then
                type -p curl >/dev/null || sudo apt install curl -y
                # Create keyrings directory if it doesn't exist (official guidance)
                sudo mkdir -p -m 755 /usr/share/keyrings
                curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
                sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
                echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null
                sudo apt update
                sudo apt install gh -y
                echo -e "${GREEN}  GitHub CLI installation complete${NC}"
            else
                echo -e "${YELLOW}  Please install GitHub CLI manually${NC}"
            fi
            ;;
        macos)
            if command_exists brew; then
                brew install gh
                echo -e "${GREEN}  GitHub CLI installation complete${NC}"
            else
                echo -e "${YELLOW}  Please install Homebrew first, then run: brew install gh${NC}"
            fi
            ;;
        windows)
            echo -e "${YELLOW}  Please install GitHub CLI from: https://cli.github.com/${NC}"
            ;;
    esac
}

# Function to restore .NET packages
restore_packages() {
    echo -e "${BLUE}[5/5] Restoring .NET packages...${NC}"

    if command_exists dotnet; then
        cd "$(dirname "$0")/.."
        dotnet restore Mystira.App.sln
        echo -e "${GREEN}  Packages restored successfully${NC}"
    else
        echo -e "${YELLOW}  Skipping package restore (.NET not available)${NC}"
    fi
}

# Function to verify installation
verify_installation() {
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}  Installation Summary${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""

    # .NET
    if command_exists dotnet; then
        echo -e "${GREEN}[OK]${NC} .NET SDK: $(dotnet --version)"
    else
        echo -e "${RED}[MISSING]${NC} .NET SDK"
    fi

    # Docker
    if command_exists docker; then
        echo -e "${GREEN}[OK]${NC} Docker: $(docker --version 2>/dev/null | head -1)"
    else
        echo -e "${YELLOW}[OPTIONAL]${NC} Docker not installed"
    fi

    # Azure CLI
    if command_exists az; then
        echo -e "${GREEN}[OK]${NC} Azure CLI: $(az --version 2>/dev/null | head -1)"
    else
        echo -e "${YELLOW}[OPTIONAL]${NC} Azure CLI not installed"
    fi

    # GitHub CLI
    if command_exists gh; then
        echo -e "${GREEN}[OK]${NC} GitHub CLI: $(gh --version 2>/dev/null | head -1)"
    else
        echo -e "${YELLOW}[OPTIONAL]${NC} GitHub CLI not installed"
    fi

    # Git
    if command_exists git; then
        echo -e "${GREEN}[OK]${NC} Git: $(git --version)"
    else
        echo -e "${RED}[MISSING]${NC} Git"
    fi

    echo ""
}

# Main execution
main() {
    install_dotnet
    echo ""
    install_docker
    echo ""
    install_azure_cli
    echo ""
    install_gh_cli
    echo ""
    restore_packages

    verify_installation

    echo -e "${GREEN}Setup complete!${NC}"
    echo ""
    echo "Next steps:"
    echo "  1. Reload your shell or run: source ~/.bashrc (or ~/.zshrc)"
    echo "  2. Run 'dotnet build' to build the solution"
    echo "  3. Run 'dotnet test' to run tests"
    echo ""
    echo "For development:"
    echo "  - API: dotnet run --project src/Mystira.App.Api"
    echo "  - PWA: dotnet run --project src/Mystira.App.PWA"
    echo ""
}

# Run main function
main "$@"
