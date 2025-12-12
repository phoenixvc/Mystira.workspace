# Mystira.App

Main user-facing applications for the Mystira platform.

## Overview

Mystira.App contains all client applications including web and mobile interfaces. This package is being modularized into distinct sub-packages for better maintainability.

## Structure

```
app/
├── web/               # Next.js web application
│   ├── src/
│   │   ├── app/      # App router pages
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── lib/
│   │   └── styles/
│   └── public/
├── mobile/            # React Native mobile app
│   ├── src/
│   │   ├── screens/
│   │   ├── components/
│   │   ├── navigation/
│   │   └── services/
│   └── ios/ & android/
└── shared/            # Shared components and utilities
    ├── ui/           # Shared UI components
    ├── hooks/        # Shared React hooks
    └── utils/        # Shared utilities
```

## Getting Started

### Web Application

```bash
cd web

# Install dependencies
pnpm install

# Start development server
pnpm dev

# Build for production
pnpm build
```

### Mobile Application

```bash
cd mobile

# Install dependencies
pnpm install

# Start Metro bundler
pnpm start

# Run on iOS
pnpm ios

# Run on Android
pnpm android
```

## Features

- User authentication and profiles
- Story exploration and interaction
- NFT gallery and marketplace
- Wallet integration
- Real-time notifications
- Social features

## Tech Stack

### Web

- Next.js 14 (App Router)
- TypeScript
- Tailwind CSS
- React Query
- Zustand

### Mobile

- React Native
- Expo
- React Navigation
- NativeWind

### Shared

- Shared design system
- Common utilities
- API client

## Environment Variables

```env
NEXT_PUBLIC_API_URL=https://api.mystira.io
NEXT_PUBLIC_WS_URL=wss://ws.mystira.io
NEXT_PUBLIC_CHAIN_ID=1
```

## Development

```bash
# Run type checking
pnpm typecheck

# Run linting
pnpm lint

# Run tests
pnpm test
```

## License

Proprietary - All rights reserved

