# Legacy Doc Migration Log

This log tracks value-adding legacy documentation migrated during monorepo
parity remediation.

## Source to target mappings

| Legacy repository | Legacy path | Monorepo target |
| --- | --- | --- |
| `phoenixvc/Mystira.App` | `docs/DOCKER_FIX_SUMMARY.md` | `packages/app/docs/operations/docker-fix-summary.md` |
| `phoenixvc/Mystira.App` | `docs/NUGET_FEED_SETUP.md` | `packages/app/docs/setup/nuget-feed-setup.md` |
| `phoenixvc/Mystira.App` | `docs/nuget/NUGET_SETUP.md` | `packages/app/docs/setup/nuget-setup.md` |
| `phoenixvc/Mystira.StoryGenerator` | `src/Mystira.StoryGenerator.RagIndexer/ENHANCED_SCHEMA_SUPPORT.md` | `packages/story-generator/docs/rag-indexer/enhanced-schema-support.md` |
| `phoenixvc/Mystira.StoryGenerator` | `src/Mystira.StoryGenerator.RagIndexer/SOLID_DRY_IMPROVEMENTS.md` | `packages/story-generator/docs/rag-indexer/solid-dry-improvements.md` |
| `phoenixvc/Mystira.Admin.Api` | `docs/NUGET_FEED_CONFIGURATION.md` | `packages/admin-api/docs/setup/nuget-feed-configuration.md` |
| `phoenixvc/Mystira.Infra` | `DNS_INGRESS_SETUP.md` | `infra/dns-ingress-setup.md` |

## Existing canonical documents retained

- `docs/architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md`
  already exists in canonical location and remains the authoritative ADR.
