# Mystira Leptos DevHub

A reactive web frontend built with [Leptos](https://leptos.dev/) for the Mystira DevHub desktop application, powered by [Tauri](https://tauri.app/).

## Prerequisites

- [Rust](https://rustup.rs/) (stable toolchain)
- [Trunk](https://trunkrs.dev/) - `cargo install trunk`
- [Node.js](https://nodejs.org/) (for Tailwind CSS)
- [Tauri CLI](https://tauri.app/) - `cargo install tauri-cli`

## Setup

1. Install dependencies:
   ```bash
   npm install
   ```

2. Build the CSS:
   ```bash
   npm run css
   ```

3. Add the WASM target:
   ```bash
   rustup target add wasm32-unknown-unknown
   ```

## Development

### Frontend only (in browser):
```bash
trunk serve
```

### Full Tauri app:
```bash
npm run tauri:dev
```

## Building

### Release build:
```bash
npm run tauri:build
```

## Project Structure

```
Mystira.Leptos/
├── src/                    # Leptos frontend source
│   ├── app.rs             # Main app component & router
│   ├── components/        # Reusable UI components
│   ├── pages/             # Page components
│   ├── state/             # Application state
│   └── tauri/             # Tauri IPC bindings
├── src-tauri/             # Tauri backend
│   ├── src/
│   │   ├── main.rs        # Tauri entry point
│   │   ├── commands.rs    # Tauri command handlers
│   │   ├── cli.rs         # .NET CLI integration
│   │   └── error.rs       # Error types
│   └── tauri.conf.json    # Tauri configuration
├── styles/                # CSS styles
├── index.html             # HTML entry point
├── Trunk.toml             # Trunk build config
└── tailwind.config.js     # Tailwind configuration
```

## Architecture

The application follows a layered architecture:

```
┌─────────────────────────────────┐
│     Leptos Frontend (WASM)      │
├─────────────────────────────────┤
│     Tauri IPC Layer (Rust)      │
├─────────────────────────────────┤
│    Tauri Backend Commands       │
├─────────────────────────────────┤
│  .NET CLI / Azure CLI / gh CLI  │
└─────────────────────────────────┘
```

## Shared Contracts

This project uses the `mystira-contracts` crate for shared type definitions between the frontend and backend. The contracts correspond to:

- `@mystira/contracts` (TypeScript/NPM)
- `Mystira.Contracts` (C#/NuGet)

## Related Documentation

- [Contracts Migration Guide](../docs/guides/contracts-migration.md)
- [Main README](../README.md)
