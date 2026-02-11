#!/bin/bash
# session-start.sh - Ensure .NET SDK is available for Claude Code sessions
# Called automatically via SessionStart hook

set -e

# Check if dotnet is already on PATH
if command -v dotnet >/dev/null 2>&1; then
    DOTNET_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    if [[ "$DOTNET_VERSION" == 9.* ]]; then
        echo ".NET SDK $DOTNET_VERSION ready"
        exit 0
    fi
fi

# Check common install locations before downloading
for DOTNET_DIR in "$HOME/.dotnet" "/usr/share/dotnet" "/usr/local/share/dotnet"; do
    if [ -x "$DOTNET_DIR/dotnet" ]; then
        VERSION=$("$DOTNET_DIR/dotnet" --version 2>/dev/null || echo "")
        if [[ "$VERSION" == 9.* ]]; then
            export DOTNET_ROOT="$DOTNET_DIR"
            export PATH="$DOTNET_DIR:$DOTNET_DIR/tools:$PATH"
            echo ".NET SDK $VERSION found at $DOTNET_DIR"

            # Persist for subsequent tool calls in this session
            if [ -f "$HOME/.bashrc" ] && ! grep -q 'DOTNET_ROOT' "$HOME/.bashrc"; then
                echo "export DOTNET_ROOT=\"$DOTNET_DIR\"" >> "$HOME/.bashrc"
                echo "export PATH=\"$DOTNET_DIR:\$DOTNET_DIR/tools:\$PATH\"" >> "$HOME/.bashrc"
            fi
            exit 0
        fi
    fi
done

# Install .NET SDK 9.0
echo "Installing .NET SDK 9.0..."
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet"

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

# Persist PATH for subsequent tool calls in this session
if [ -f "$HOME/.bashrc" ] && ! grep -q 'DOTNET_ROOT' "$HOME/.bashrc"; then
    echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.bashrc"
    echo 'export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"' >> "$HOME/.bashrc"
fi

# Restore NuGet packages
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
if [ -f "$REPO_ROOT/Mystira.App.sln" ]; then
    echo "Restoring NuGet packages..."
    "$HOME/.dotnet/dotnet" restore "$REPO_ROOT/Mystira.App.sln" --verbosity quiet
fi

echo ".NET SDK $(dotnet --version) installed and ready"
