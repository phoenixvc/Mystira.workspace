# COPPA Compliance Audit

Review code for COPPA (Children's Online Privacy Protection Act) compliance. This is a CRITICAL requirement for Mystira as a children's platform.

## Arguments

- `$ARGUMENTS` - Optional scope:
  - A file or directory path to audit
  - A feature name (e.g., `registration`, `game-sessions`, `profiles`)
  - `--full` for a complete codebase audit
  - If omitted, audits recently changed files

## Instructions

### 1. COPPA Requirements Reference

Reference: `docs/prd/features/coppa-compliance.md`

**Core COPPA Rules (16 CFR Part 312):**

1. **Parental Consent** - Must obtain verifiable parental consent before collecting PII from children under 13
2. **Privacy Notice** - Must provide clear, prominent privacy notice
3. **Data Minimization** - Only collect PII reasonably necessary for the activity
4. **Data Security** - Maintain reasonable security for children's PII
5. **Data Retention** - Retain PII only as long as necessary
6. **Parental Access** - Parents can review, delete, or refuse further collection
7. **No Conditioning** - Cannot condition participation on unnecessary PII disclosure

### 2. What to Scan For

#### PII Collection Points

Search for any code that collects, stores, or transmits:

- Full name, email address, physical address
- Phone numbers, Social Security numbers
- Geolocation data (GPS, IP-based location)
- Photos, videos, audio recordings
- Persistent identifiers (cookies, device IDs) used for tracking
- Screen names that could reveal real identity

**Search patterns:**

- Form inputs collecting personal data
- API endpoints accepting PII in request bodies
- Database fields storing PII
- Third-party services receiving user data

#### Missing Consent Checks

Look for:

- User creation/registration flows WITHOUT age verification
- Profile updates that collect additional PII without consent check
- Features that enable direct communication between users (chat, messaging)
- Any data sharing with third parties without consent

#### Data Retention Issues

Look for:

- PII stored without TTL or expiration policy
- Logs containing PII without redaction (known issue: BUG-4)
- Backup/export features that include children's PII
- Analytics or telemetry collecting persistent identifiers

#### Security Gaps

Look for:

- PII transmitted without encryption
- PII stored in plain text (not encrypted at rest)
- Missing access controls on PII endpoints
- Overly broad API responses including unnecessary PII

### 3. Audit Checklist

For each file/feature audited, evaluate:

- [ ] **Age Gate**: Is there an age verification step before PII collection?
- [ ] **Parental Consent**: Is verifiable parental consent obtained?
- [ ] **Data Minimization**: Is only necessary PII collected?
- [ ] **Purpose Limitation**: Is PII used only for the stated purpose?
- [ ] **Secure Storage**: Is PII encrypted at rest?
- [ ] **Secure Transit**: Is PII encrypted in transit (HTTPS)?
- [ ] **Access Control**: Are PII endpoints properly authorized?
- [ ] **PII Redaction**: Are logs and error messages free of PII?
- [ ] **Retention Policy**: Is there a defined retention/deletion policy?
- [ ] **Parent Access**: Can parents view/delete their child's data?
- [ ] **Third-Party Sharing**: Is any PII shared with third parties?
- [ ] **Direct Contact**: Are there any direct contact features (chat, email)?

### 4. Output Format

```
## COPPA Compliance Audit Report

### Scope
- Files audited: X
- Features reviewed: [list]

### Risk Level: [COMPLIANT | LOW RISK | MEDIUM RISK | HIGH RISK | CRITICAL]

### Findings

#### [CRITICAL] PII Collection Without Consent
- **Location:** src/Mystira.App.Api/Controllers/UserProfilesController.cs
- **Issue:** Email address collected during profile creation without age verification or parental consent
- **COPPA Rule:** 16 CFR 312.5 - Verifiable parental consent required
- **Recommendation:** Add age gate before profile creation; require parental consent for users under 13

#### [HIGH] PII in Logs
- **Location:** src/Mystira.App.Api/Program.cs (logging middleware)
- **Issue:** Request logging may capture PII in request bodies
- **COPPA Rule:** 16 CFR 312.8 - Confidentiality and security
- **Recommendation:** Add PII redaction filter to logging pipeline

### Implementation Status
Reference: docs/prd/features/coppa-compliance.md

| Requirement | Status | Notes |
|---|---|---|
| Age Gate | NOT IMPLEMENTED | Blocker for launch |
| Parental Consent | NOT IMPLEMENTED | Blocker for launch |
| Parent Dashboard | NOT IMPLEMENTED | Needed for data access |
| PII Encryption | PARTIAL | Transit yes, at-rest needs audit |
| Log Redaction | NOT IMPLEMENTED | BUG-4 tracked |

### Recommendations (Priority Order)
1. ...
2. ...
```

### 5. After Audit

- Flag any CRITICAL findings that are launch blockers
- Reference existing PRD documentation for implementation guidance
- Suggest concrete next steps for remediation
- Note: COPPA violations carry $53,088 per violation (FTC adjustment effective Jan 17, 2025)
