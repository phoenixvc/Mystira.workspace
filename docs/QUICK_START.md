# Quick Start Guide

Get up and running with the Mystira workspace in 5 minutes.

## Prerequisites Check

```bash
# Check Node.js version (should be 18+)
node --version

# Check pnpm version (should be 8+)
pnpm --version

# Check Docker (optional, for local services)
docker --version
```

## Step 1: Clone the Workspace

```bash
git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
cd Mystira.workspace
```

If you already cloned without submodules:

```bash
git submodule update --init --recursive
```

## Step 2: Install Dependencies

```bash
pnpm install
```

## Step 3: Start Local Services (Optional)

```bash
# Start PostgreSQL and Redis
docker-compose up -d

# Verify services are running
docker-compose ps
```

## Step 4: Set Up Environment Variables

```bash
# Copy example environment files (if they exist in submodules)
# Each submodule may have its own .env.example
```

## Step 5: Build and Run

```bash
# Build all packages
pnpm build

# Start development servers
pnpm dev
```

## Common Commands

```bash
# Run specific package
pnpm --filter @mystira/chain dev
pnpm --filter @mystira/app-web dev
pnpm --filter @mystira/story-generator dev

# Run tests
pnpm test

# Lint code
pnpm lint

# Format code
pnpm format
```

## Troubleshooting

### Submodules Not Initialized

```bash
git submodule update --init --recursive
```

### Dependencies Issues

```bash
# Clear and reinstall
rm -rf node_modules
pnpm install
```

### Port Conflicts

If ports are already in use:
- Change ports in `docker-compose.yml`
- Update environment variables accordingly

## Next Steps

- Read [SETUP.md](./SETUP.md) for detailed setup instructions
- Check [SUBMODULES.md](./SUBMODULES.md) for submodule management
- Review [ARCHITECTURE.md](./ARCHITECTURE.md) for system overview
- See [ENVIRONMENT.md](./ENVIRONMENT.md) for environment variables

## Getting Help

- Check existing issues on GitHub
- Review documentation in `docs/` directory
- Contact maintainers

