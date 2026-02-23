# Secret Rotation Runbook

This document describes the secret rotation procedures for the Mystira infrastructure.

## Overview

With the migration to OIDC authentication, most Azure-related credentials no longer require rotation. This runbook covers the remaining secrets that may need periodic rotation.

## Secret Inventory

### Secrets That DON'T Require Rotation (OIDC)

| Secret                  | Reason                           |
| ----------------------- | -------------------------------- |
| `AZURE_CLIENT_ID`       | Not a secret - public identifier |
| `AZURE_TENANT_ID`       | Not a secret - public identifier |
| `AZURE_SUBSCRIPTION_ID` | Not a secret - public identifier |

These are static identifiers that don't expire. The actual authentication uses short-lived tokens issued by GitHub's OIDC provider.

### Secrets That MAY Require Rotation

| Secret                 | Rotation Frequency    | Notes                           |
| ---------------------- | --------------------- | ------------------------------- |
| `GH_PACKAGES_TOKEN`    | 90 days (recommended) | GitHub PAT for package registry |
| `INFRACOST_API_KEY`    | Per provider policy   | Infracost API key               |
| `SLACK_WEBHOOK_URL`    | When compromised      | Slack incoming webhook          |
| `TEAMS_WEBHOOK_URL`    | When compromised      | Microsoft Teams webhook         |
| `MS_TEAMS_WEBHOOK_URL` | When compromised      | Legacy Teams webhook            |

### Deprecated Secrets

| Secret                                  | Status     | Migration Guide                                             |
| --------------------------------------- | ---------- | ----------------------------------------------------------- |
| `MYSTIRA_AZURE_CREDENTIALS`             | Deprecated | See [OIDC_MIGRATION.md](OIDC_MIGRATION.md)                  |
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | Deprecated | No longer needed — monorepo migration eliminated submodules |

## Automated Monitoring

The `secret-rotation-check.yml` workflow runs weekly and monitors:

- Azure AD credential expiration
- GitHub token validity
- API key validity
- Webhook configuration

It creates GitHub issues when action is required.

## Rotation Procedures

### GitHub Personal Access Tokens (PATs)

#### MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN (Deprecated)

> **Deprecated**: This token was used for submodule checkout. Since the monorepo migration inlined all submodule code, this token is no longer needed. It can be safely removed from repository secrets.

#### GH_PACKAGES_TOKEN

This token accesses GitHub Packages for NuGet packages.

**Required Scopes:**

- `read:packages`
- `write:packages` (if publishing)

**Rotation Steps:**
Same as above, with different permissions configuration.

### API Keys

#### INFRACOST_API_KEY

**Rotation Steps:**

1. Log into [Infracost Cloud](https://dashboard.infracost.io/)
2. Navigate to Settings → API Keys
3. Generate new API key
4. Update GitHub secret: `INFRACOST_API_KEY`
5. Verify by running cost estimation workflow
6. Revoke old key

### Webhook URLs

Webhook URLs don't expire but should be rotated if:

- The URL is accidentally exposed
- Team members with access leave
- Security audit requires it

#### SLACK_WEBHOOK_URL

**Rotation Steps:**

1. In Slack workspace admin:
   - Go to **Apps** → **Incoming Webhooks**
   - Find the existing webhook and note the channel
   - Create new webhook for the same channel
   - Copy new URL

2. Update GitHub secret: `SLACK_WEBHOOK_URL`

3. Test by triggering a workflow with notifications

4. Disable old webhook in Slack admin

#### TEAMS_WEBHOOK_URL / MS_TEAMS_WEBHOOK_URL

**Rotation Steps:**

1. In Microsoft Teams:
   - Open the channel
   - Click **...** → **Connectors**
   - Find **Incoming Webhook**
   - Create new webhook or regenerate URL

2. Update GitHub secret

3. Test notifications

4. Remove old webhook connector

## Emergency Rotation

If a secret is compromised:

1. **Immediately rotate the affected secret** using procedures above

2. **Review access logs:**
   - GitHub: Repository → Insights → Traffic → Git clones
   - Azure: Azure AD → Sign-in logs
   - Slack: Workspace Analytics

3. **Check for unauthorized usage:**
   - Review recent workflow runs
   - Check Azure activity logs
   - Review Slack/Teams messages

4. **Document the incident:**
   - Create GitHub issue with `security` label
   - Document timeline and remediation

## Verification Checklist

After rotation, verify:

- [ ] Workflow using the secret runs successfully
- [ ] No unauthorized access detected in logs
- [ ] Old credential is revoked/disabled
- [ ] Documentation updated if procedures changed
- [ ] Team notified if relevant

## Monitoring and Alerts

### Automated Checks

The `secret-rotation-check.yml` workflow:

- Runs weekly on Monday at 9 AM UTC
- Creates issues when rotation is needed
- Sends Slack alerts for urgent items

### Manual Verification

Periodically verify:

1. All PATs are within 90 days of creation
2. Webhook URLs are still valid
3. API keys haven't been revoked
4. OIDC configuration is working

## Best Practices

1. **Use short-lived tokens** - Set PAT expiration to 90 days max
2. **Principle of least privilege** - Only grant necessary scopes
3. **Document rotation** - Note when secrets were last rotated
4. **Automate verification** - Use the rotation check workflow
5. **Separate environments** - Use different secrets per environment when possible
6. **Mask in logs** - Ensure secrets are masked in workflow logs

## Related Documentation

- [OIDC Migration Guide](OIDC_MIGRATION.md)
- [GitHub Encrypted Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [GitHub PAT Best Practices](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens)
