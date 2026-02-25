# Monorepo Parity Issue Register

This register tracks remaining parity concerns from manifest-backed analysis and
records the recovery decision per item.

## Decision legend

- `recover`: restore missing artifact/behavior.
- `intentional-drift`: changed by design; keep with rationale.
- `owner-confirm`: likely intentional but requires explicit owner sign-off.

## Active items

| ID      | Repo           | Legacy path / concern                                                   | Current evidence                                                                                                                                                 | Type                  | Decision          | Status | Evidence                                                                                                                                                              |
| ------- | -------------- | ----------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ----------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PAR-001 | StoryGenerator | `src/Mystira.StoryGenerator.Domain/Commands/ICommandHandler.cs` missing | Legacy MediatR adapter interface replaced by orchestrator + Wolverine-style handler model (`IAgentOrchestrator`, `*CommandHandler.cs`, shared `ICommand` marker) | code/behavior         | intentional-drift | Closed | `mystira-storygenerator-legacy-tree-recursive.json`, `mystira-storygenerator-monorepo-target-tree-recursive.json`, `docs/planning/adr-0015-implementation-roadmap.md` |
| PAR-002 | App            | `global.json` missing from package target                               | Legacy repo had `global.json`; monorepo package target omits it while workspace root has `global.json` (`path: global.json`)                                     | config/runtime policy | intentional-drift | Closed | `mystira-app-legacy-tree-recursive.json`, `mystira-app-monorepo-target-tree-recursive.json`, `mystira-workspace-tree-recursive.json`                                  |
| PAR-003 | Admin UI       | `vite.config.js` and `vite.config.d.ts` missing                         | Legacy `vite.config.js` replaced by `packages/admin-ui/vite.config.ts`; runtime config values align (port/proxy/build aliases preserved)                         | config/build          | intentional-drift | Closed | `mystira-admin-ui-legacy-tree-recursive.json`, `mystira-admin-ui-monorepo-target-tree-recursive.json`                                                                 |
| PAR-004 | Publisher      | `eslint.config.js` missing                                              | `packages/publisher/eslint.config.mjs` present with matching manifest blob SHA (`d7538df...`) to legacy `eslint.config.js`                                       | config/lint           | intentional-drift | Closed | `mystira-publisher-legacy-tree-recursive.json`, `mystira-publisher-monorepo-target-tree-recursive.json`                                                               |
| PAR-005 | Infra          | `terraform/environments/dev/.terraform/modules/modules.json` missing    | Generated local Terraform metadata should not be tracked                                                                                                         | infra/generated       | intentional-drift | Closed | `mystira-infra-legacy-tree-recursive.json`, `mystira-infra-monorepo-target-tree-recursive.json`                                                                       |

## Decision notes

- PAR-002: `.NET` SDK pinning policy is centralized at monorepo root `global.json`
  rather than duplicated per package.
- PAR-003: `vite.config.d.ts` was type-side support for JS config and is no longer
  needed after moving to typed `vite.config.ts`.
- PAR-004: extension moved from `.js` to `.mjs` with identical content hash, so
  behavior is preserved.
- PAR-001: legacy `ICommandHandler<T>` inherited MediatR request handlers; in the
  monorepo the command pipeline is orchestrator-driven (`IAgentOrchestrator`) and
  concrete `*CommandHandler` implementations are discovered via conventions.

## Documentation migration queue

Migrated valid/value-adding legacy docs into canonical monorepo taxonomy:

- App -> `packages/app/docs/operations/docker-fix-summary.md`,
  `packages/app/docs/setup/nuget-feed-setup.md`,
  `packages/app/docs/setup/nuget-setup.md`
- StoryGenerator ->
  `packages/story-generator/docs/rag-indexer/enhanced-schema-support.md`,
  `packages/story-generator/docs/rag-indexer/solid-dry-improvements.md`
- Admin API -> `packages/admin-api/docs/setup/nuget-feed-configuration.md`
- Infra -> `infra/dns-ingress-setup.md`
- Existing canonical ADR retained:
  `docs/architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md`

## Exit criteria

- All former `owner-confirm` items have explicit decision notes.
- No unresolved functional `recover` items remain.
- Documentation migration queue is migrated and indexed.
