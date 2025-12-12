# Environment Variables

This document describes all environment variables used across the Mystira workspace.

## Root Workspace Variables

Create a `.env.local` file in the root directory with these variables:

```env
# Node Environment
NODE_ENV=development

# Workspace Configuration
WORKSPACE_ROOT=.
```

## Mystira.Chain Variables

Located in `packages/chain/.env.local`:

```env
# Blockchain Network
CHAIN_ID=1337
RPC_URL=http://localhost:8545

# Deployment
PRIVATE_KEY=your_deployer_private_key
INFURA_API_KEY=your_infura_key
ETHERSCAN_API_KEY=your_etherscan_key

# Network URLs
MAINNET_RPC_URL=
TESTNET_RPC_URL=
```

## Mystira.App Variables

### Web Application (`packages/app/web/.env.local`)

```env
# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:3000/api
NEXT_PUBLIC_WS_URL=ws://localhost:3001

# Blockchain
NEXT_PUBLIC_CHAIN_ID=1337
NEXT_PUBLIC_RPC_URL=http://localhost:8545

# Authentication
NEXT_PUBLIC_AUTH_URL=http://localhost:3000/auth

# Analytics (optional)
NEXT_PUBLIC_ANALYTICS_ID=
```

### Mobile Application (`packages/app/mobile/.env`)

```env
# API Configuration
API_URL=http://localhost:3000/api
WS_URL=ws://localhost:3001

# Blockchain
CHAIN_ID=1337
RPC_URL=http://localhost:8545
```

## Mystira.StoryGenerator Variables

Located in `packages/story-generator/.env.local`:

```env
# AI Providers
ANTHROPIC_API_KEY=sk-ant-...
OPENAI_API_KEY=sk-...

# Generation Settings
DEFAULT_MODEL=claude-3-opus
MAX_TOKENS=4096
TEMPERATURE=0.8
TOP_P=0.9

# Database
DATABASE_URL=postgresql://mystira:mystira_dev@localhost:5432/mystira_dev
REDIS_URL=redis://localhost:6379

# API Configuration
PORT=3001
API_KEY=your_api_key_here
```

## Mystira.Infra Variables

### Terraform (`infra/terraform/.env`)

```env
# Cloud Provider
AWS_ACCESS_KEY_ID=
AWS_SECRET_ACCESS_KEY=
AWS_REGION=us-east-1

# Or for GCP
GOOGLE_APPLICATION_CREDENTIALS=
GCP_PROJECT_ID=
GCP_REGION=us-central1
```

### Kubernetes

Kubernetes secrets are managed via `kubectl` or Helm charts. See `infra/kubernetes/` for details.

## Docker Compose

The root `docker-compose.yml` uses these default values (can be overridden):

```env
POSTGRES_USER=mystira
POSTGRES_PASSWORD=mystira_dev
POSTGRES_DB=mystira_dev
REDIS_PASSWORD=
```

## Security Notes

⚠️ **Never commit `.env.local` files to git!**

- All `.env.local` files are in `.gitignore`
- Use `.env.example` files as templates
- Use secret management in production (Vault, AWS Secrets Manager, etc.)
- Rotate API keys regularly
- Use different keys for development/staging/production

## Environment-Specific Files

- `.env.local` - Local development (gitignored)
- `.env.development` - Development environment
- `.env.staging` - Staging environment
- `.env.production` - Production environment (managed via infrastructure)

## Loading Order

Environment variables are loaded in this order (later overrides earlier):

1. System environment variables
2. `.env` file
3. `.env.local` file
4. `.env.[NODE_ENV]` file
5. `.env.[NODE_ENV].local` file

