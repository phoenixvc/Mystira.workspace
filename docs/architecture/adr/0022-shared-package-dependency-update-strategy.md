# ADR-0022: Shared Package Dependency Update Strategy

## Status

**Proposed** - 2025-12-24

## Context

When shared packages (like `Mystira.Contracts`) are updated in one repository, consuming applications (like `mystira.app`) need to be updated to use the new version. Currently this is a manual process that creates friction and delays.

### Current State

**Package Flow**:
1. Developer changes contracts in `Mystira.workspace`
2. PR merged → CI publishes new NuGet package version
3. Developer manually updates package version in `mystira.app`
4. Developer creates PR in `mystira.app`
5. PR merged → Changes deployed

**Problems**:
- Manual process is error-prone and easy to forget
- Delays between package publish and consumer updates
- Breaking changes discovered late in the cycle
- No visibility into which consumers use which versions

### Requirements

1. **Automation**: Reduce manual intervention
2. **Speed**: Minimize time between package publish and consumer update
3. **Visibility**: Clear tracking of package versions across repos
4. **Safety**: Breaking changes should be caught early
5. **Flexibility**: Support different update policies per package

## Options Analysis

### Option 1: Automated Dependency Updates (Renovate/Dependabot)

**Description**: Use automated tools to create PRs when dependencies update.

**Flow**:
1. Package published to NuGet feed
2. Bot detects new version
3. Bot creates PR in consuming repo with updated version
4. CI runs tests against new version
5. Developer reviews and merges

### Option 2: Monorepo

**Description**: Consolidate all repositories into a single monorepo.

**Flow**:
1. All code in one repository
2. Changes to shared code are atomic with consumers
3. Single PR contains all related changes
4. Single CI pipeline validates everything

### Option 3: Git Submodules with Source References

**Description**: Use git submodules and project references instead of packages.

**Flow**:
1. Consumer repos include shared code as submodule
2. Use `<ProjectReference>` instead of `<PackageReference>`
3. Update submodule pointer when shared code changes
4. Build from source

### Option 4: CI Chained PRs

**Description**: Custom CI workflow that triggers PRs in downstream repos.

**Flow**:
1. Package published
2. CI workflow in source repo triggers workflow in consumer repos
3. Consumer workflow creates PR with updated version
4. Tests run automatically

### Option 5: Manual Updates (Current)

**Description**: Developers manually update package versions.

**Flow**:
1. Developer notices new package version
2. Developer updates version in consuming project
3. Developer creates PR

## Weighted Evaluation Matrix

### Criteria Weights

| Criterion | Weight | Rationale |
|-----------|--------|-----------|
| **Automation** | 25% | Primary goal is reducing manual work |
| **Time to Update** | 20% | Speed of propagating changes matters |
| **Breaking Change Detection** | 20% | Catching issues early is critical |
| **Setup Complexity** | 15% | Initial effort to implement |
| **Maintenance Burden** | 10% | Ongoing effort to maintain |
| **Flexibility** | 10% | Ability to customize per-package |

### Scoring (1-5, higher is better)

| Criterion | Weight | Renovate/Dependabot | Monorepo | Submodules | CI Chained | Manual |
|-----------|--------|---------------------|----------|------------|------------|--------|
| Automation | 25% | 5 | 5 | 3 | 4 | 1 |
| Time to Update | 20% | 4 | 5 | 3 | 4 | 2 |
| Breaking Change Detection | 20% | 4 | 5 | 4 | 4 | 2 |
| Setup Complexity | 15% | 5 | 2 | 3 | 2 | 5 |
| Maintenance Burden | 10% | 4 | 3 | 2 | 2 | 4 |
| Flexibility | 10% | 5 | 2 | 3 | 4 | 5 |

### Weighted Scores

| Option | Calculation | Total |
|--------|-------------|-------|
| **Renovate/Dependabot** | (5×0.25)+(4×0.20)+(4×0.20)+(5×0.15)+(4×0.10)+(5×0.10) | **4.50** |
| **Monorepo** | (5×0.25)+(5×0.20)+(5×0.20)+(2×0.15)+(3×0.10)+(2×0.10) | **4.05** |
| **Submodules** | (3×0.25)+(3×0.20)+(4×0.20)+(3×0.15)+(2×0.10)+(3×0.10) | **3.10** |
| **CI Chained** | (4×0.25)+(4×0.20)+(4×0.20)+(2×0.15)+(2×0.10)+(4×0.10) | **3.50** |
| **Manual** | (1×0.25)+(2×0.20)+(2×0.20)+(5×0.15)+(4×0.10)+(5×0.10) | **2.70** |

### Ranking

1. **Renovate/Dependabot: 4.50** ✅ Recommended
2. Monorepo: 4.05
3. CI Chained PRs: 3.50
4. Git Submodules: 3.10
5. Manual: 2.70

## Renovate vs Dependabot: Detailed Comparison

Both tools automate dependency updates, but have significant differences.

### Feature Comparison

| Feature | Dependabot | Renovate |
|---------|------------|----------|
| **Provider** | GitHub (built-in) | Mend (formerly WhiteSource) |
| **Hosting** | GitHub-managed only | Self-hosted or Mend-hosted |
| **Configuration** | YAML (`.github/dependabot.yml`) | JSON (`.renovate.json` or `renovate.json`) |
| **Supported Ecosystems** | 16+ (npm, NuGet, Docker, etc.) | 90+ ecosystems |
| **Automerge** | Via GitHub Actions only | Native support |
| **Grouping** | Limited (same ecosystem only) | Advanced (regex, patterns) |
| **Scheduling** | Basic (daily/weekly/monthly) | Cron expressions, timezone-aware |
| **Custom Versioning** | Limited | Extensive regex support |
| **Monorepo Support** | Basic | Advanced |
| **Presets/Sharing** | None | Shareable config presets |
| **Dashboard** | None | Dependency Dashboard issue |
| **Replacement Rules** | None | Package replacement/renaming |
| **Post-upgrade Commands** | None | Custom scripts after update |
| **Vulnerability Alerts** | Integrated with GitHub Security | Via Mend platform |
| **Rate Limiting** | Fixed limits | Configurable |
| **PR Limits** | Configurable per ecosystem | Global and per-package |

### Weighted Comparison: Renovate vs Dependabot

#### Criteria Weights

| Criterion | Weight | Rationale |
|-----------|--------|-----------|
| **Ecosystem Support** | 15% | More ecosystems = more coverage |
| **Configuration Flexibility** | 20% | Customization is key for complex needs |
| **Automerge Capability** | 15% | Reduces manual intervention |
| **Grouping/Batching** | 15% | Reduces PR noise |
| **Setup Simplicity** | 15% | Lower barrier to entry |
| **Maintenance/Reliability** | 10% | Ongoing operational burden |
| **Cost** | 10% | Budget considerations |

#### Scoring (1-5, higher is better)

| Criterion | Weight | Dependabot | Renovate |
|-----------|--------|------------|----------|
| Ecosystem Support | 15% | 3 | 5 |
| Configuration Flexibility | 20% | 2 | 5 |
| Automerge Capability | 15% | 3 | 5 |
| Grouping/Batching | 15% | 2 | 5 |
| Setup Simplicity | 15% | 5 | 3 |
| Maintenance/Reliability | 10% | 5 | 4 |
| Cost | 10% | 5 | 4 |

#### Weighted Scores

| Tool | Calculation | Total |
|------|-------------|-------|
| **Renovate** | (5×0.15)+(5×0.20)+(5×0.15)+(5×0.15)+(3×0.15)+(4×0.10)+(4×0.10) | **4.35** |
| **Dependabot** | (3×0.15)+(2×0.20)+(3×0.15)+(2×0.15)+(5×0.15)+(5×0.10)+(5×0.10) | **3.15** |

### Detailed Analysis

#### Dependabot Advantages

1. **Zero Setup**: Built into GitHub, just add config file
2. **Native Integration**: Security alerts, dependency graph, Insights
3. **No External Dependencies**: No third-party service to manage
4. **Free**: Included with GitHub at all tiers
5. **Reliability**: Maintained by GitHub, always available

#### Dependabot Limitations

1. **Limited Grouping**: Cannot group updates across ecosystems
2. **No Automerge**: Requires separate GitHub Action for automerge
3. **Basic Scheduling**: Only daily/weekly/monthly
4. **No Dashboard**: No central view of pending updates
5. **No Post-Update Hooks**: Cannot run scripts after update
6. **No Config Sharing**: Must duplicate config across repos

#### Renovate Advantages

1. **Powerful Grouping**: Group by regex, package patterns, update type
2. **Native Automerge**: Built-in with configurable rules
3. **Dependency Dashboard**: Issue-based tracking of all updates
4. **Shareable Presets**: Reuse config across organization
5. **Post-Upgrade Commands**: Run scripts after package updates
6. **Package Replacement**: Handle package renames/deprecations
7. **Flexible Scheduling**: Full cron support with timezones
8. **Extensive Customization**: Regex versioning, custom managers

#### Renovate Limitations

1. **More Complex Setup**: Requires app installation or self-hosting
2. **Learning Curve**: More configuration options to understand
3. **External Dependency**: Relies on Mend's hosted service (or self-host)
4. **Occasional Bugs**: More features = more edge cases

### Use Case Recommendations

| Scenario | Recommendation | Rationale |
|----------|----------------|-----------|
| Simple projects, few dependencies | **Dependabot** | Zero setup, good enough |
| Monorepo with many packages | **Renovate** | Better grouping, dashboard |
| Custom versioning schemes | **Renovate** | Regex versioning support |
| Wanting automerge for patches | **Renovate** | Native automerge rules |
| Multiple repos, shared config | **Renovate** | Preset sharing |
| Security-focused, minimal config | **Dependabot** | Native GitHub Security integration |
| Mixed ecosystems (NuGet + npm + Docker) | **Renovate** | Better cross-ecosystem grouping |
| Enterprise with compliance needs | **Either** | Both support approval workflows |

## Decision

**Primary**: Use **Renovate** for automated dependency updates across all repositories.

**Rationale**:

1. **Better for our multi-repo structure**: Shareable presets reduce duplication
2. **Superior grouping**: Can group Mystira.* packages together
3. **Native automerge**: Reduces manual work for patch/minor updates
4. **Dependency Dashboard**: Central visibility into update status
5. **Post-upgrade commands**: Can run `dotnet restore` or tests automatically

### Implementation Plan

#### Phase 1: Renovate Setup (Week 1)

1. Install Renovate GitHub App on `phoenixvc` organization
2. Create shared preset in `Mystira.workspace`:

```json
// renovate-config.json (shared preset)
{
  "$schema": "https://docs.renovatebot.com/renovate-schema.json",
  "extends": [
    "config:recommended",
    ":semanticCommits"
  ],
  "packageRules": [
    {
      "matchPackagePatterns": ["^Mystira\\."],
      "groupName": "Mystira packages",
      "automerge": false,
      "schedule": ["after 9am on monday"]
    },
    {
      "matchUpdateTypes": ["patch"],
      "automerge": true,
      "automergeType": "pr"
    }
  ],
  "dependencyDashboard": true,
  "labels": ["dependencies"],
  "prHourlyLimit": 5,
  "prConcurrentLimit": 10
}
```

3. Add to consuming repos (e.g., `mystira.app`):

```json
// renovate.json
{
  "extends": [
    "github>phoenixvc/Mystira.workspace//renovate-config"
  ]
}
```

#### Phase 2: Configure Per-Repo Rules (Week 2)

1. Set update schedules appropriate for each repo
2. Configure automerge policies
3. Set up PR reviewers/assignees
4. Enable Dependency Dashboard

#### Phase 3: Monitor and Refine (Ongoing)

1. Monitor PR volume and adjust limits
2. Refine grouping rules based on patterns
3. Add post-upgrade commands as needed
4. Update shared preset based on learnings

### Fallback: Dependabot

If Renovate proves too complex or unreliable, fallback to Dependabot:

```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "daily"
    allow:
      - dependency-name: "Mystira.*"
    labels:
      - "dependencies"
      - "nuget"
    commit-message:
      prefix: "deps"
```

## Consequences

### Positive

1. **Automated Updates**: PRs created automatically when packages update
2. **Early Detection**: Breaking changes caught immediately via CI
3. **Reduced Manual Work**: No more remembering to update versions
4. **Visibility**: Dashboard shows pending updates across all repos
5. **Consistency**: Shared config ensures uniform behavior

### Negative

1. **PR Volume**: May create many PRs if not configured carefully
2. **Learning Curve**: Team needs to understand Renovate configuration
3. **External Dependency**: Relies on Renovate service availability
4. **False Positives**: Some updates may break unexpectedly

### Mitigations

1. **Rate Limiting**: Configure `prHourlyLimit` and `prConcurrentLimit`
2. **Grouping**: Group related packages to reduce PR count
3. **Documentation**: Create team guide for Renovate workflow
4. **Automerge Wisely**: Only automerge patch updates initially

## Related ADRs

- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0016: Monorepo Tooling and Multi-Repository Strategy](./0016-monorepo-tooling-and-multi-repository-strategy.md)
- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md)

## References

- [Renovate Documentation](https://docs.renovatebot.com/)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [Renovate vs Dependabot Comparison](https://docs.renovatebot.com/bot-comparison/)
- [Renovate Presets](https://docs.renovatebot.com/config-presets/)
- [Renovate for NuGet](https://docs.renovatebot.com/modules/manager/nuget/)
