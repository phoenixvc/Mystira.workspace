# User Domain PostgreSQL Migration

## Overview

This document outlines the migration of the User Domain from Cosmos DB to PostgreSQL as part of the hybrid data strategy defined in [ADR-0013](../adr/0013-data-management-and-storage-strategy.md).

## PostgreSQL Schema Design

### Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         USER DOMAIN SCHEMA                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐     1:N     ┌─────────────────┐                    │
│  │    accounts     │◄───────────►│  user_profiles  │                    │
│  ├─────────────────┤             ├─────────────────┤                    │
│  │ id (PK)         │             │ id (PK)         │                    │
│  │ auth0_user_id   │             │ account_id (FK) │                    │
│  │ email           │             │ name            │                    │
│  │ display_name    │             │ age_group       │                    │
│  │ role            │             │ date_of_birth   │                    │
│  │ created_at      │             │ is_guest        │                    │
│  │ last_login_at   │             │ is_npc          │                    │
│  │ settings (JSONB)│             │ pronouns        │                    │
│  └────────┬────────┘             │ bio             │                    │
│           │                      │ avatar_media_id │                    │
│           │                      │ themes (JSONB)  │                    │
│           ▼                      │ created_at      │                    │
│  ┌─────────────────┐             │ updated_at      │                    │
│  │  subscriptions  │             │ onboarded       │                    │
│  ├─────────────────┤             └────────┬────────┘                    │
│  │ id (PK)         │                      │                             │
│  │ account_id (FK) │                      │ 1:N                         │
│  │ type            │                      ▼                             │
│  │ product_id      │             ┌─────────────────┐                    │
│  │ tier            │             │  user_badges    │                    │
│  │ is_active       │             ├─────────────────┤                    │
│  │ valid_until     │             │ id (PK)         │                    │
│  │ start_date      │             │ profile_id (FK) │                    │
│  │ end_date        │             │ badge_config_id │                    │
│  │ purchase_token  │             │ badge_name      │                    │
│  │ last_verified   │             │ badge_message   │                    │
│  │ purchased_items │             │ axis            │                    │
│  └─────────────────┘             │ trigger_value   │                    │
│                                  │ threshold       │                    │
│  ┌─────────────────┐             │ earned_at       │                    │
│  │ pending_signups │             │ game_session_id │                    │
│  ├─────────────────┤             │ scenario_id     │                    │
│  │ id (PK)         │             │ image_id        │                    │
│  │ email           │             └─────────────────┘                    │
│  │ display_name    │                                                    │
│  │ code            │             ┌─────────────────┐                    │
│  │ created_at      │             │ completed_      │                    │
│  │ expires_at      │             │ scenarios       │                    │
│  │ is_used         │             ├─────────────────┤                    │
│  │ is_signin       │             │ id (PK)         │                    │
│  │ failed_attempts │             │ account_id (FK) │                    │
│  └─────────────────┘             │ scenario_id     │                    │
│                                  │ completed_at    │                    │
│                                  └─────────────────┘                    │
└─────────────────────────────────────────────────────────────────────────┘
```

### SQL Schema

```sql
-- =============================================================================
-- USER DOMAIN POSTGRESQL SCHEMA
-- Mystira.App User Domain Migration
-- =============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =============================================================================
-- ACCOUNTS TABLE
-- =============================================================================

CREATE TABLE accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    auth0_user_id VARCHAR(255) UNIQUE,
    email VARCHAR(255) NOT NULL,
    display_name VARCHAR(255) NOT NULL DEFAULT '',
    role VARCHAR(50) NOT NULL DEFAULT 'Guest',
    settings JSONB NOT NULL DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_login_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT accounts_email_check CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z]{2,}$'),
    CONSTRAINT accounts_role_check CHECK (role IN ('Guest', 'User', 'Admin', 'SuperAdmin'))
);

-- Indexes for accounts
CREATE INDEX idx_accounts_email ON accounts(email);
CREATE INDEX idx_accounts_auth0_user_id ON accounts(auth0_user_id);
CREATE INDEX idx_accounts_role ON accounts(role);
CREATE INDEX idx_accounts_created_at ON accounts(created_at);

-- =============================================================================
-- SUBSCRIPTIONS TABLE
-- =============================================================================

CREATE TYPE subscription_type AS ENUM ('Free', 'Monthly', 'Annual', 'Lifetime', 'Individual');

CREATE TABLE subscriptions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id UUID NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    type subscription_type NOT NULL DEFAULT 'Free',
    product_id VARCHAR(255),
    tier VARCHAR(50) NOT NULL DEFAULT 'Free',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    valid_until TIMESTAMPTZ,
    start_date TIMESTAMPTZ DEFAULT NOW(),
    end_date TIMESTAMPTZ,
    purchase_token TEXT,
    last_verified TIMESTAMPTZ,
    purchased_scenarios TEXT[] DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Only one active subscription per account
    CONSTRAINT subscriptions_account_unique UNIQUE (account_id)
);

-- Indexes for subscriptions
CREATE INDEX idx_subscriptions_account_id ON subscriptions(account_id);
CREATE INDEX idx_subscriptions_is_active ON subscriptions(is_active);
CREATE INDEX idx_subscriptions_type ON subscriptions(type);
CREATE INDEX idx_subscriptions_valid_until ON subscriptions(valid_until) WHERE valid_until IS NOT NULL;

-- =============================================================================
-- USER PROFILES TABLE
-- =============================================================================

CREATE TABLE user_profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id UUID REFERENCES accounts(id) ON DELETE SET NULL,
    name VARCHAR(255) NOT NULL DEFAULT '',
    age_group VARCHAR(20) NOT NULL DEFAULT '6-9',
    date_of_birth DATE,
    is_guest BOOLEAN NOT NULL DEFAULT FALSE,
    is_npc BOOLEAN NOT NULL DEFAULT FALSE,
    pronouns VARCHAR(50),
    bio TEXT,
    avatar_media_id VARCHAR(255),
    selected_avatar_media_id VARCHAR(255),
    preferred_themes JSONB NOT NULL DEFAULT '[]',
    has_completed_onboarding BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Constraints
    CONSTRAINT user_profiles_age_group_check CHECK (
        age_group ~ '^[0-9]+-[0-9]+$'
    )
);

-- Indexes for user_profiles
CREATE INDEX idx_user_profiles_account_id ON user_profiles(account_id);
CREATE INDEX idx_user_profiles_is_guest ON user_profiles(is_guest);
CREATE INDEX idx_user_profiles_is_npc ON user_profiles(is_npc);
CREATE INDEX idx_user_profiles_age_group ON user_profiles(age_group);
CREATE INDEX idx_user_profiles_created_at ON user_profiles(created_at);

-- =============================================================================
-- USER BADGES TABLE
-- =============================================================================

CREATE TABLE user_badges (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_profile_id UUID NOT NULL REFERENCES user_profiles(id) ON DELETE CASCADE,
    badge_configuration_id VARCHAR(255) NOT NULL,
    badge_id VARCHAR(255),
    badge_name VARCHAR(255) NOT NULL,
    badge_message TEXT NOT NULL DEFAULT '',
    axis VARCHAR(100) NOT NULL,
    trigger_value REAL NOT NULL DEFAULT 0,
    threshold REAL NOT NULL DEFAULT 0,
    earned_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    game_session_id VARCHAR(255),
    scenario_id VARCHAR(255),
    image_id VARCHAR(255) NOT NULL DEFAULT '',

    -- Prevent duplicate badges
    CONSTRAINT user_badges_unique UNIQUE (user_profile_id, badge_configuration_id)
);

-- Indexes for user_badges
CREATE INDEX idx_user_badges_profile_id ON user_badges(user_profile_id);
CREATE INDEX idx_user_badges_axis ON user_badges(axis);
CREATE INDEX idx_user_badges_earned_at ON user_badges(earned_at);
CREATE INDEX idx_user_badges_badge_config_id ON user_badges(badge_configuration_id);

-- =============================================================================
-- COMPLETED SCENARIOS TABLE (Junction table)
-- =============================================================================

CREATE TABLE completed_scenarios (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id UUID NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    scenario_id VARCHAR(255) NOT NULL,
    completed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Prevent duplicates
    CONSTRAINT completed_scenarios_unique UNIQUE (account_id, scenario_id)
);

-- Indexes for completed_scenarios
CREATE INDEX idx_completed_scenarios_account_id ON completed_scenarios(account_id);
CREATE INDEX idx_completed_scenarios_scenario_id ON completed_scenarios(scenario_id);

-- =============================================================================
-- PENDING SIGNUPS TABLE
-- =============================================================================

CREATE TABLE pending_signups (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL,
    display_name VARCHAR(255) NOT NULL DEFAULT '',
    code VARCHAR(10) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    is_signin BOOLEAN NOT NULL DEFAULT FALSE,
    failed_attempts INTEGER NOT NULL DEFAULT 0,

    -- Constraints
    CONSTRAINT pending_signups_email_check CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z]{2,}$'),
    CONSTRAINT pending_signups_failed_attempts_check CHECK (failed_attempts >= 0 AND failed_attempts <= 10)
);

-- Indexes for pending_signups
CREATE INDEX idx_pending_signups_email ON pending_signups(email);
CREATE INDEX idx_pending_signups_code ON pending_signups(code);
CREATE INDEX idx_pending_signups_expires_at ON pending_signups(expires_at);
CREATE INDEX idx_pending_signups_is_used ON pending_signups(is_used);

-- Cleanup old pending signups (can be called by a scheduled job)
CREATE OR REPLACE FUNCTION cleanup_expired_signups()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM pending_signups
    WHERE expires_at < NOW() - INTERVAL '24 hours';

    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- AUDIT TRIGGERS
-- =============================================================================

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply trigger to tables with updated_at
CREATE TRIGGER update_subscriptions_updated_at
    BEFORE UPDATE ON subscriptions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_user_profiles_updated_at
    BEFORE UPDATE ON user_profiles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================================================
-- VIEWS FOR COMMON QUERIES
-- =============================================================================

-- View: Active subscriptions
CREATE VIEW active_subscriptions AS
SELECT
    s.*,
    a.email,
    a.display_name
FROM subscriptions s
JOIN accounts a ON s.account_id = a.id
WHERE s.is_active = TRUE
  AND (s.valid_until IS NULL OR s.valid_until > NOW());

-- View: User profiles with account info
CREATE VIEW user_profiles_with_account AS
SELECT
    p.*,
    a.email AS account_email,
    a.display_name AS account_display_name,
    a.role AS account_role,
    s.type AS subscription_type,
    s.tier AS subscription_tier,
    s.is_active AS subscription_active
FROM user_profiles p
LEFT JOIN accounts a ON p.account_id = a.id
LEFT JOIN subscriptions s ON a.id = s.account_id;

-- =============================================================================
-- SAMPLE DATA MIGRATION FUNCTION
-- =============================================================================

-- Function to migrate from Cosmos DB format (called from application code)
CREATE OR REPLACE FUNCTION migrate_account_from_cosmos(
    p_cosmos_id VARCHAR(255),
    p_auth0_user_id VARCHAR(255),
    p_email VARCHAR(255),
    p_display_name VARCHAR(255),
    p_role VARCHAR(50),
    p_settings JSONB,
    p_created_at TIMESTAMPTZ,
    p_last_login_at TIMESTAMPTZ
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO accounts (
        id, auth0_user_id, email, display_name, role, settings, created_at, last_login_at
    ) VALUES (
        p_cosmos_id::UUID, p_auth0_user_id, p_email, p_display_name, p_role,
        COALESCE(p_settings, '{}'), p_created_at, p_last_login_at
    )
    ON CONFLICT (id) DO UPDATE SET
        auth0_user_id = EXCLUDED.auth0_user_id,
        email = EXCLUDED.email,
        display_name = EXCLUDED.display_name,
        role = EXCLUDED.role,
        settings = EXCLUDED.settings,
        last_login_at = EXCLUDED.last_login_at
    RETURNING id INTO new_id;

    RETURN new_id;
END;
$$ LANGUAGE plpgsql;
```

## EF Core DbContext Configuration

```csharp
// PostgreSqlDbContext.cs - New context for PostgreSQL
public class PostgreSqlDbContext : DbContext
{
    public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<CompletedScenario> CompletedScenarios { get; set; }
    public DbSet<PendingSignup> PendingSignups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Auth0UserId).HasColumnName("auth0_user_id");
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Settings).HasColumnName("settings")
                  .HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");

            entity.HasOne(e => e.Subscription)
                  .WithOne(s => s.Account)
                  .HasForeignKey<Subscription>(s => s.AccountId);

            entity.HasMany(e => e.UserProfiles)
                  .WithOne(p => p.Account)
                  .HasForeignKey(p => p.AccountId);

            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Auth0UserId).IsUnique();
        });

        // Subscription configuration
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasColumnName("type")
                  .HasConversion<string>();
            entity.Property(e => e.PurchasedScenarios).HasColumnName("purchased_scenarios")
                  .HasColumnType("text[]");
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredThemes).HasColumnName("preferred_themes")
                  .HasColumnType("jsonb");

            entity.HasMany(e => e.EarnedBadges)
                  .WithOne(b => b.UserProfile)
                  .HasForeignKey(b => b.UserProfileId);
        });

        // UserBadge configuration
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.ToTable("user_badges");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserProfileId, e.BadgeConfigurationId })
                  .IsUnique();
        });

        // PendingSignup configuration
        modelBuilder.Entity<PendingSignup>(entity =>
        {
            entity.ToTable("pending_signups");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Code);
        });
    }
}
```

## Dual-Write Pattern Implementation

### Phase 1: Write to Both, Read from Cosmos DB

```csharp
public class DualWriteAccountRepository : IAccountRepository
{
    private readonly MystiraAppDbContext _cosmosContext;
    private readonly PostgreSqlDbContext _postgresContext;
    private readonly ILogger<DualWriteAccountRepository> _logger;

    public async Task<Account> CreateAsync(Account account)
    {
        // Primary: Write to Cosmos DB
        _cosmosContext.Accounts.Add(account);
        await _cosmosContext.SaveChangesAsync();

        // Secondary: Write to PostgreSQL (async, don't block)
        _ = Task.Run(async () =>
        {
            try
            {
                var pgAccount = MapToPostgres(account);
                _postgresContext.Accounts.Add(pgAccount);
                await _postgresContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write account {Id} to PostgreSQL", account.Id);
                // Queue for retry
            }
        });

        return account;
    }

    public async Task<Account?> GetByIdAsync(string id)
    {
        // Read from Cosmos DB (primary)
        return await _cosmosContext.Accounts.FindAsync(id);
    }
}
```

### Phase 2: Write to Both, Read from PostgreSQL

```csharp
public class DualWriteAccountRepositoryPhase2 : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(string id)
    {
        // Read from PostgreSQL (new primary)
        var pgAccount = await _postgresContext.Accounts.FindAsync(Guid.Parse(id));

        if (pgAccount == null)
        {
            // Fallback to Cosmos DB during transition
            var cosmosAccount = await _cosmosContext.Accounts.FindAsync(id);
            if (cosmosAccount != null)
            {
                // Backfill PostgreSQL
                await SyncToPostgres(cosmosAccount);
                return cosmosAccount;
            }
        }

        return MapFromPostgres(pgAccount);
    }
}
```

## Migration Checklist

### Phase 1: Foundation
- [ ] Create PostgreSQL database in shared PostgreSQL server
- [ ] Run schema migration scripts
- [ ] Create EF Core PostgreSQL context
- [ ] Implement dual-write repositories
- [ ] Add feature flags for migration phases

### Phase 2: Data Migration
- [ ] Create migration job to copy existing Cosmos DB data
- [ ] Validate data integrity between sources
- [ ] Monitor write latency and errors
- [ ] Set up alerting for sync failures

### Phase 3: Cutover
- [ ] Switch read path to PostgreSQL
- [ ] Keep Cosmos DB writes for rollback
- [ ] Monitor for 1 week
- [ ] Disable Cosmos DB writes
- [ ] Archive Cosmos DB data

## Rollback Plan

1. Feature flag to switch back to Cosmos DB reads
2. PostgreSQL continues to receive writes during rollback
3. Manual re-sync if needed after rollback
