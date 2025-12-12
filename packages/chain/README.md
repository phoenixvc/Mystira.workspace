# Mystira.Chain

Blockchain infrastructure, smart contracts, and Web3 integration for the Mystira platform.

## Overview

Mystira.Chain handles all blockchain-related functionality including:

- Smart contract development and deployment
- NFT collection management
- Token economics and staking
- Web3 wallet integration
- On-chain game mechanics

## Structure

```
chain/
├── contracts/          # Smart contracts (Solidity/Move)
│   ├── nft/           # NFT-related contracts
│   ├── token/         # Token contracts
│   ├── staking/       # Staking mechanisms
│   └── game/          # On-chain game logic
├── scripts/           # Deployment and utility scripts
├── test/              # Contract tests
├── typechain/         # Generated TypeScript types
└── hardhat.config.ts  # Hardhat configuration
```

## Getting Started

```bash
# Install dependencies
pnpm install

# Compile contracts
pnpm compile

# Run tests
pnpm test

# Deploy to local network
pnpm deploy:local
```

## Development

### Prerequisites

- Hardhat or Foundry
- Node.js 18+
- A Web3 wallet for testing

### Environment Variables

```env
PRIVATE_KEY=your_deployer_private_key
INFURA_API_KEY=your_infura_key
ETHERSCAN_API_KEY=your_etherscan_key
```

### Testing

```bash
# Run all tests
pnpm test

# Run with coverage
pnpm test:coverage

# Run specific test file
pnpm test test/NFT.test.ts
```

### Deployment

```bash
# Deploy to testnet
pnpm deploy:testnet

# Deploy to mainnet
pnpm deploy:mainnet

# Verify contracts
pnpm verify
```

## Security

- All contracts undergo security audits before mainnet deployment
- Bug bounty program available for critical vulnerabilities
- Follow secure development practices

## License

Proprietary - All rights reserved

