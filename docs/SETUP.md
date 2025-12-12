# Setup Guide

Quick start guide for setting up the Mystira workspace.

## Initial Setup

1. **Clone the repository with submodules** (if not already done):
   ```bash
   git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
   cd Mystira.workspace
   ```
   
   If you've already cloned without submodules:
   ```bash
   git submodule update --init --recursive
   ```

2. **Install pnpm** (if not already installed):
   ```bash
   npm install -g pnpm@8.10.0
   ```

3. **Install dependencies**:
   ```bash
   pnpm install
   ```

4. **Set up environment variables**:
   ```bash
   # Create .env.local from example
   cp .env.example .env.local
   # Edit .env.local with your actual values
   ```

## Development Workflow

### Running All Services

```bash
# Start all packages in development mode
pnpm dev
```

### Running Individual Packages

```bash
# Chain (blockchain)
pnpm --filter @mystira/chain dev

# App Web
pnpm --filter @mystira/app-web dev

# App Mobile
pnpm --filter @mystira/app-mobile start

# Story Generator
pnpm --filter @mystira/story-generator dev
```

### Building

```bash
# Build all packages
pnpm build

# Build specific package
pnpm --filter @mystira/chain build
```

### Testing

```bash
# Run all tests
pnpm test

# Run tests for specific package
pnpm --filter @mystira/chain test
```

## Package-Specific Setup

### Mystira.Chain

1. Install Hardhat dependencies:
   ```bash
   cd packages/chain
   pnpm install
   ```

2. Set up environment variables in `.env.local`:
   ```
   PRIVATE_KEY=your_key
   INFURA_API_KEY=your_key
   ```

3. Compile contracts:
   ```bash
   pnpm compile
   ```

### Mystira.App

#### Web

1. Navigate to web package:
   ```bash
   cd packages/app/web
   ```

2. Install dependencies (if not done at root):
   ```bash
   pnpm install
   ```

3. Start development server:
   ```bash
   pnpm dev
   ```

#### Mobile

1. Navigate to mobile package:
   ```bash
   cd packages/app/mobile
   ```

2. Install dependencies:
   ```bash
   pnpm install
   ```

3. Start Expo:
   ```bash
   pnpm start
   ```

### Mystira.StoryGenerator

1. Navigate to story generator:
   ```bash
   cd packages/story-generator
   ```

2. Set up environment variables:
   ```
   ANTHROPIC_API_KEY=your_key
   OPENAI_API_KEY=your_key
   ```

3. Start development server:
   ```bash
   pnpm dev
   ```

### Mystira.Infra

1. Navigate to infra:
   ```bash
   cd infra
   ```

2. Initialize Terraform (if using):
   ```bash
   cd terraform/environments/dev
   terraform init
   ```

## Troubleshooting

### pnpm Issues

If you encounter pnpm workspace issues:
```bash
# Clear pnpm store
pnpm store prune

# Reinstall
rm -rf node_modules
pnpm install
```

### TypeScript Errors

If you see TypeScript errors:
```bash
# Run type checking
pnpm typecheck

# Clear TypeScript cache
rm -rf node_modules/.cache
```

### Build Issues

If builds fail:
```bash
# Clean all build artifacts
pnpm clean

# Rebuild
pnpm build
```

## Next Steps

- Read the [Contributing Guide](../CONTRIBUTING.md)
- Check individual package READMEs for specific instructions
- Review the [Architecture Overview](../README.md#architecture)

