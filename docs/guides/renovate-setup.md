# Renovate Setup Guide

This guide explains how to configure Renovate for Mystira repositories.

## Prerequisites

1. Install the [Renovate GitHub App](https://github.com/apps/renovate) on the `phoenixvc` organization
2. Grant access to the repositories you want Renovate to manage

## For Consuming Repositories (e.g., mystira.app)

Add a `renovate.json` file to the repository root:

```json
{
  "$schema": "https://docs.renovatebot.com/renovate-schema.json",
  "extends": [
    "github>phoenixvc/Mystira.workspace//renovate-config"
  ]
}
```

This extends the shared preset from `Mystira.workspace`, which provides:

- Automatic grouping of Mystira packages
- Automerge for patch updates
- Automerge for trusted Microsoft/Azure minor updates
- Dependency Dashboard for visibility
- Security vulnerability prioritization

### Customizing for Your Repo

You can override or extend the shared config:

```json
{
  "$schema": "https://docs.renovatebot.com/renovate-schema.json",
  "extends": [
    "github>phoenixvc/Mystira.workspace//renovate-config"
  ],
  "schedule": ["after 10am on monday"],
  "packageRules": [
    {
      "description": "Disable automerge for this repo",
      "matchPackagePatterns": ["*"],
      "automerge": false
    }
  ]
}
```

## Shared Preset Features

The shared preset (`renovate-config.json`) includes:

### Package Grouping

| Group | Packages |
|-------|----------|
| Mystira packages | `Mystira.*` |
| Microsoft packages | `Microsoft.*` |
| Azure SDK packages | `Azure.*` |
| Testing packages | `xunit`, `Moq`, `FluentAssertions`, etc. |
| Linting packages | `eslint`, `prettier`, `@typescript-eslint` |
| TypeScript tooling | `typescript`, `vite`, `vitest`, etc. |
| Terraform providers | All Terraform providers |
| GitHub Actions | All GitHub Actions |

### Automerge Rules

| Update Type | Automerge? | Condition |
|-------------|------------|-----------|
| Patch | Yes | Non-Mystira packages |
| Minor | Yes | Microsoft, Azure, Serilog, Swashbuckle |
| Major | No | Requires manual approval |
| Mystira packages | No | Always requires review |
| Security fixes | No | Prioritized but requires review |

### Schedule

- Default: 8am-6pm weekdays (Africa/Johannesburg timezone)
- Mystira packages: Any time (prioritized)

## Dependency Dashboard

Renovate creates an issue titled "Renovate Dependency Dashboard" in each repo. This shows:

- Pending updates
- Open PRs
- Detected problems
- Package adoption status

## Troubleshooting

### PRs Not Being Created

1. Check Renovate has access to the repo
2. Check the Dependency Dashboard issue for errors
3. Verify `renovate.json` syntax at [Renovate Config Validator](https://docs.renovatebot.com/config-validation/)

### Too Many PRs

Adjust limits in your `renovate.json`:

```json
{
  "extends": ["github>phoenixvc/Mystira.workspace//renovate-config"],
  "prConcurrentLimit": 5,
  "prHourlyLimit": 2
}
```

### Automerge Not Working

Ensure:
1. Branch protection allows Renovate to merge
2. Required status checks are passing
3. `platformAutomerge` is enabled (default in shared config)

## Updating the Shared Preset

To modify the shared preset, edit `renovate-config.json` in `Mystira.workspace` and merge to `main`. All consuming repos will automatically use the updated config.

## References

- [Renovate Documentation](https://docs.renovatebot.com/)
- [ADR-0022: Shared Package Dependency Update Strategy](../architecture/adr/0022-shared-package-dependency-update-strategy.md)
- [Renovate Config Presets](https://docs.renovatebot.com/config-presets/)
