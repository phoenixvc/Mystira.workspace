# Mystira Workspace Backlog

Single source of truth for open work items in the Mystira monorepo.

## Active Work Items

### Documentation & Organization

- [ ] Create `BACKLOG.md` as single source of truth for open work items
- [ ] Update `.github/workflows/README.md` to reflect current 13 workflows
- [ ] Flatten workflow trigger table and add reusable workflow tracking
- [ ] Remove stale migration/review docs (consolidated into backlog)

### Features In Progress

- [ ] App test gaps refactor
- [ ] Devhub Leptos Tauri integration

### Dependencies (Renovate)

- [ ] rust:1.93-slim-bookworm
- [ ] python:3.14-slim
- [ ] python:3.11-slim
- [ ] github-actions updates
- [ ] node.js updates
- [ ] debian:bookworm-slim
- [ ] @testing-library/react to v16
- [ ] microsoft packages v3 (major)
- [ ] linting packages
- [ ] all non-major dependencies

## Completed

- [x] Submodules converted to true monorepo structure
- [x] .NET 10.0 upgrade across all packages
- [x] ESLint converted to modular configuration format

## Notes

- All migration docs consolidated in `docs/migrations/`
- Package-specific CI runs via turbo in workspace root
- Release workflows run centrally from workspace
