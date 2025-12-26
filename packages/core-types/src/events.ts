/**
 * Domain events for Mystira platform.
 * Mirrors Mystira.Shared.Messaging.Events in C#.
 */

/**
 * Base interface for all domain events.
 */
export interface DomainEvent {
  /** When the event occurred (ISO 8601) */
  occurredAt: string;
}

/**
 * Base interface for integration events (cross-service).
 */
export interface IntegrationEvent extends DomainEvent {
  /** Unique event ID for idempotency */
  eventId: string;
}

// =============================================================================
// Account Events
// =============================================================================

/**
 * Published when a new user account is created.
 */
export interface AccountCreated extends IntegrationEvent {
  type: 'AccountCreated';
  accountId: string;
  email: string;
  provider?: string;
}

/**
 * Published when a user account is updated.
 */
export interface AccountUpdated extends IntegrationEvent {
  type: 'AccountUpdated';
  accountId: string;
  updatedFields: string[];
}

/**
 * Published when a user account is deleted.
 */
export interface AccountDeleted extends IntegrationEvent {
  type: 'AccountDeleted';
  accountId: string;
  isSoftDelete: boolean;
}

// =============================================================================
// Session Events
// =============================================================================

/**
 * Published when a game session is started.
 */
export interface SessionStarted extends IntegrationEvent {
  type: 'SessionStarted';
  sessionId: string;
  accountId: string;
  scenarioId: string;
}

/**
 * Published when a game session is completed.
 */
export interface SessionCompleted extends IntegrationEvent {
  type: 'SessionCompleted';
  sessionId: string;
  accountId: string;
  scenarioId: string;
  durationSeconds: number;
  outcome?: string;
}

/**
 * Published when a session is abandoned.
 */
export interface SessionAbandoned extends IntegrationEvent {
  type: 'SessionAbandoned';
  sessionId: string;
  accountId: string;
  durationSeconds: number;
  lastProgressPoint?: string;
}

// =============================================================================
// Content Events
// =============================================================================

/**
 * Published when a scenario is created.
 */
export interface ScenarioCreated extends IntegrationEvent {
  type: 'ScenarioCreated';
  scenarioId: string;
  title: string;
  authorId: string;
  isPublished: boolean;
}

/**
 * Published when a scenario is updated.
 */
export interface ScenarioUpdated extends IntegrationEvent {
  type: 'ScenarioUpdated';
  scenarioId: string;
  updatedFields: string[];
  version?: number;
}

/**
 * Published when a scenario is published.
 */
export interface ScenarioPublished extends IntegrationEvent {
  type: 'ScenarioPublished';
  scenarioId: string;
  title: string;
  authorId: string;
}

/**
 * Published when a scenario is unpublished.
 */
export interface ScenarioUnpublished extends IntegrationEvent {
  type: 'ScenarioUnpublished';
  scenarioId: string;
  reason?: string;
}

/**
 * Published when media is uploaded.
 */
export interface MediaUploaded extends IntegrationEvent {
  type: 'MediaUploaded';
  mediaId: string;
  uploaderId: string;
  mimeType: string;
  sizeBytes: number;
}

// =============================================================================
// Cache Events
// =============================================================================

/**
 * Published when cache should be invalidated.
 */
export interface CacheInvalidated extends IntegrationEvent {
  type: 'CacheInvalidated';
  keyPattern: string;
  entityType: string;
  entityId?: string;
  sourceService: string;
}

/**
 * Published when cache warmup is requested.
 */
export interface CacheWarmupRequested extends IntegrationEvent {
  type: 'CacheWarmupRequested';
  entityType: string;
  entityIds?: string[];
  priority: number;
}

// =============================================================================
// AI Events
// =============================================================================

/**
 * Published when story generation is requested.
 */
export interface StoryGenerationRequested extends IntegrationEvent {
  type: 'StoryGenerationRequested';
  requestId: string;
  scenarioId: string;
  accountId: string;
  prompt?: string;
}

/**
 * Published when story generation completes successfully.
 */
export interface StoryGenerationCompleted extends IntegrationEvent {
  type: 'StoryGenerationCompleted';
  requestId: string;
  scenarioId: string;
  durationMs: number;
  tokensUsed?: number;
  model?: string;
}

/**
 * Published when story generation fails.
 */
export interface StoryGenerationFailed extends IntegrationEvent {
  type: 'StoryGenerationFailed';
  requestId: string;
  scenarioId: string;
  errorCode: string;
  errorMessage: string;
  isRetryable: boolean;
}

// =============================================================================
// User Events
// =============================================================================

/**
 * Published when a user logs in.
 */
export interface UserLoggedIn extends IntegrationEvent {
  type: 'UserLoggedIn';
  accountId: string;
  provider: string;
  clientIp?: string;
  userAgent?: string;
}

/**
 * Published when a user logs out.
 */
export interface UserLoggedOut extends IntegrationEvent {
  type: 'UserLoggedOut';
  accountId: string;
  isExplicit: boolean;
}

/**
 * Published when a password reset is requested.
 */
export interface PasswordResetRequested extends IntegrationEvent {
  type: 'PasswordResetRequested';
  accountId: string;
  email: string;
  expiresAt: string;
}

/**
 * Published when a password is successfully changed.
 */
export interface PasswordChanged extends IntegrationEvent {
  type: 'PasswordChanged';
  accountId: string;
  viaReset: boolean;
}

// =============================================================================
// Notification Events
// =============================================================================

/**
 * Published when a notification is sent.
 */
export interface NotificationSent extends IntegrationEvent {
  type: 'NotificationSent';
  notificationId: string;
  recipientId: string;
  notificationType: string;
  template: string;
}

/**
 * Published when an email is sent.
 */
export interface EmailSent extends IntegrationEvent {
  type: 'EmailSent';
  emailId: string;
  recipientEmail: string;
  template: string;
  subject: string;
}

/**
 * Published when a notification delivery fails.
 */
export interface NotificationFailed extends IntegrationEvent {
  type: 'NotificationFailed';
  notificationId: string;
  recipientId: string;
  reason: string;
  isRetryable: boolean;
}

// =============================================================================
// Progression Events
// =============================================================================

/**
 * Published when a user makes a choice in a story.
 */
export interface ChoiceMade extends IntegrationEvent {
  type: 'ChoiceMade';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  nodeId: string;
  choiceId: string;
  decisionTimeSeconds?: number;
}

/**
 * Published when a user starts a chapter.
 */
export interface ChapterStarted extends IntegrationEvent {
  type: 'ChapterStarted';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  chapterId: string;
  chapterNumber: number;
  isFirstPlay: boolean;
}

/**
 * Published when a user completes a chapter.
 */
export interface ChapterCompleted extends IntegrationEvent {
  type: 'ChapterCompleted';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  chapterId: string;
  chapterNumber: number;
  durationSeconds: number;
  choicesMade: number;
  outcome?: string;
}

/**
 * Published when a user reaches a checkpoint.
 */
export interface CheckpointReached extends IntegrationEvent {
  type: 'CheckpointReached';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  checkpointId: string;
  checkpointName?: string;
  progressPercent: number;
}

/**
 * Published when user takes a specific story branch.
 */
export interface StoryBranchTaken extends IntegrationEvent {
  type: 'StoryBranchTaken';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  branchId: string;
  branchName?: string;
  fromNodeId: string;
  toNodeId: string;
}

/**
 * Published when user explicitly saves their progress.
 */
export interface ProgressSaved extends IntegrationEvent {
  type: 'ProgressSaved';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  saveSlotId: string;
  currentNodeId: string;
  progressPercent: number;
  isOverwrite: boolean;
}

// =============================================================================
// Gamification Events
// =============================================================================

/**
 * Published when a user unlocks an achievement.
 */
export interface AchievementUnlocked extends IntegrationEvent {
  type: 'AchievementUnlocked';
  accountId: string;
  achievementId: string;
  achievementName: string;
  category: string;
  rarity?: string;
  xpReward: number;
  scenarioId?: string;
}

/**
 * Published when a user earns a badge.
 */
export interface BadgeEarned extends IntegrationEvent {
  type: 'BadgeEarned';
  accountId: string;
  badgeId: string;
  badgeName: string;
  tier?: string;
  earnedVia?: string;
}

/**
 * Published when a user earns experience points.
 */
export interface XPEarned extends IntegrationEvent {
  type: 'XPEarned';
  accountId: string;
  amount: number;
  source: string;
  totalXP: number;
  multiplier: number;
  relatedEntityId?: string;
}

/**
 * Published when a user levels up.
 */
export interface LevelUp extends IntegrationEvent {
  type: 'LevelUp';
  accountId: string;
  fromLevel: number;
  toLevel: number;
  totalXP: number;
  unlockedRewards?: string[];
}

/**
 * Published when a user's streak is updated.
 */
export interface StreakUpdated extends IntegrationEvent {
  type: 'StreakUpdated';
  accountId: string;
  streakType: string;
  currentStreak: number;
  previousStreak: number;
  longestStreak: number;
  wasBroken: boolean;
  bonusXP: number;
}

/**
 * Published when leaderboard position changes.
 */
export interface LeaderboardUpdated extends IntegrationEvent {
  type: 'LeaderboardUpdated';
  accountId: string;
  leaderboardId: string;
  previousRank: number;
  newRank: number;
  score: number;
  period: string;
}

// =============================================================================
// Payment Events
// =============================================================================

/**
 * Published when a subscription is started.
 */
export interface SubscriptionStarted extends IntegrationEvent {
  type: 'SubscriptionStarted';
  accountId: string;
  subscriptionId: string;
  planId: string;
  planName: string;
  billingInterval: string;
  amountCents: number;
  currency: string;
  isTrial: boolean;
  trialEndsAt?: string;
}

/**
 * Published when a subscription is renewed.
 */
export interface SubscriptionRenewed extends IntegrationEvent {
  type: 'SubscriptionRenewed';
  accountId: string;
  subscriptionId: string;
  paymentId: string;
  amountCents: number;
  currency: string;
  nextBillingDate: string;
}

/**
 * Published when a subscription is cancelled.
 */
export interface SubscriptionCancelled extends IntegrationEvent {
  type: 'SubscriptionCancelled';
  accountId: string;
  subscriptionId: string;
  reason?: string;
  isImmediate: boolean;
  accessEndsAt: string;
  refundRequested: boolean;
}

/**
 * Published when a payment succeeds.
 */
export interface PaymentSucceeded extends IntegrationEvent {
  type: 'PaymentSucceeded';
  accountId: string;
  paymentId: string;
  amountCents: number;
  currency: string;
  paymentMethod: string;
  productType: string;
  productId: string;
  externalTransactionId?: string;
}

/**
 * Published when a payment fails.
 */
export interface PaymentFailed extends IntegrationEvent {
  type: 'PaymentFailed';
  accountId: string;
  paymentId: string;
  amountCents: number;
  currency: string;
  failureCode: string;
  failureMessage: string;
  isRetryable: boolean;
  retryAttempt: number;
}

/**
 * Published when a refund is processed.
 */
export interface RefundProcessed extends IntegrationEvent {
  type: 'RefundProcessed';
  accountId: string;
  refundId: string;
  originalPaymentId: string;
  amountCents: number;
  currency: string;
  reason: string;
  isPartial: boolean;
}

/**
 * Published when premium content is unlocked.
 */
export interface PremiumContentUnlocked extends IntegrationEvent {
  type: 'PremiumContentUnlocked';
  accountId: string;
  contentType: string;
  contentId: string;
  unlockMethod: string;
  paymentId?: string;
}

// =============================================================================
// Social Events
// =============================================================================

/**
 * Published when a user rates a scenario.
 */
export interface ScenarioRated extends IntegrationEvent {
  type: 'ScenarioRated';
  accountId: string;
  scenarioId: string;
  rating: number;
  previousRating?: number;
}

/**
 * Published when a user reviews a scenario.
 */
export interface ScenarioReviewed extends IntegrationEvent {
  type: 'ScenarioReviewed';
  reviewId: string;
  accountId: string;
  scenarioId: string;
  rating: number;
  hasTextContent: boolean;
  textLength: number;
  hasCompletedScenario: boolean;
}

/**
 * Published when a user shares a scenario.
 */
export interface ScenarioShared extends IntegrationEvent {
  type: 'ScenarioShared';
  accountId: string;
  scenarioId: string;
  platform: string;
  shareId: string;
}

/**
 * Published when a user follows another user.
 */
export interface UserFollowed extends IntegrationEvent {
  type: 'UserFollowed';
  followerAccountId: string;
  followedAccountId: string;
  isMutual: boolean;
}

/**
 * Published when a user unfollows another user.
 */
export interface UserUnfollowed extends IntegrationEvent {
  type: 'UserUnfollowed';
  followerAccountId: string;
  unfollowedAccountId: string;
}

/**
 * Published when a comment is posted.
 */
export interface CommentPosted extends IntegrationEvent {
  type: 'CommentPosted';
  commentId: string;
  accountId: string;
  targetType: string;
  targetId: string;
  parentCommentId?: string;
  textLength: number;
}

/**
 * Published when a comment is deleted.
 */
export interface CommentDeleted extends IntegrationEvent {
  type: 'CommentDeleted';
  commentId: string;
  accountId: string;
  deletedBy: string;
  reason?: string;
}

/**
 * Published when a user likes content.
 */
export interface ContentLiked extends IntegrationEvent {
  type: 'ContentLiked';
  accountId: string;
  contentType: string;
  contentId: string;
  contentOwnerId: string;
}

/**
 * Published when a user unlikes content.
 */
export interface ContentUnliked extends IntegrationEvent {
  type: 'ContentUnliked';
  accountId: string;
  contentType: string;
  contentId: string;
}

// =============================================================================
// Moderation Events
// =============================================================================

/**
 * Published when content is reported by a user.
 */
export interface ContentReported extends IntegrationEvent {
  type: 'ContentReported';
  reportId: string;
  reporterAccountId: string;
  contentType: string;
  contentId: string;
  contentOwnerId: string;
  category: string;
  hasDetails: boolean;
}

/**
 * Published when content is moderated.
 */
export interface ContentModerated extends IntegrationEvent {
  type: 'ContentModerated';
  actionId: string;
  contentType: string;
  contentId: string;
  contentOwnerId: string;
  moderatorId: string;
  action: string;
  reason: string;
  relatedReportIds?: string[];
}

/**
 * Published when a user receives a warning.
 */
export interface UserWarned extends IntegrationEvent {
  type: 'UserWarned';
  warningId: string;
  accountId: string;
  moderatorId: string;
  category: string;
  severity: string;
  warningCount: number;
  relatedContentId?: string;
}

/**
 * Published when a user is suspended.
 */
export interface UserSuspended extends IntegrationEvent {
  type: 'UserSuspended';
  suspensionId: string;
  accountId: string;
  moderatorId: string;
  reason: string;
  durationHours?: number;
  endsAt?: string;
  canAppeal: boolean;
}

/**
 * Published when a user is banned.
 */
export interface UserBanned extends IntegrationEvent {
  type: 'UserBanned';
  banId: string;
  accountId: string;
  moderatorId: string;
  reason: string;
  isPermanent: boolean;
  endsAt?: string;
  canAppeal: boolean;
}

/**
 * Published when a user submits an appeal.
 */
export interface AppealSubmitted extends IntegrationEvent {
  type: 'AppealSubmitted';
  appealId: string;
  accountId: string;
  appealType: string;
  relatedActionId: string;
}

/**
 * Published when an appeal is resolved.
 */
export interface AppealResolved extends IntegrationEvent {
  type: 'AppealResolved';
  appealId: string;
  accountId: string;
  moderatorId: string;
  outcome: string;
  notes?: string;
}

// =============================================================================
// Union Types
// =============================================================================

/**
 * All account-related events.
 */
export type AccountEvent = AccountCreated | AccountUpdated | AccountDeleted;

/**
 * All session-related events.
 */
export type SessionEvent = SessionStarted | SessionCompleted | SessionAbandoned;

/**
 * All content-related events.
 */
export type ContentEvent =
  | ScenarioCreated
  | ScenarioUpdated
  | ScenarioPublished
  | ScenarioUnpublished
  | MediaUploaded;

/**
 * All cache-related events.
 */
export type CacheEvent = CacheInvalidated | CacheWarmupRequested;

/**
 * All AI-related events.
 */
export type AIEvent = StoryGenerationRequested | StoryGenerationCompleted | StoryGenerationFailed;

/**
 * All user authentication events.
 */
export type UserEvent = UserLoggedIn | UserLoggedOut | PasswordResetRequested | PasswordChanged;

/**
 * All notification-related events.
 */
export type NotificationEvent = NotificationSent | EmailSent | NotificationFailed;

/**
 * All progression-related events.
 */
export type ProgressionEvent =
  | ChoiceMade
  | ChapterStarted
  | ChapterCompleted
  | CheckpointReached
  | StoryBranchTaken
  | ProgressSaved;

/**
 * All gamification-related events.
 */
export type GamificationEvent =
  | AchievementUnlocked
  | BadgeEarned
  | XPEarned
  | LevelUp
  | StreakUpdated
  | LeaderboardUpdated;

/**
 * All payment-related events.
 */
export type PaymentEvent =
  | SubscriptionStarted
  | SubscriptionRenewed
  | SubscriptionCancelled
  | PaymentSucceeded
  | PaymentFailed
  | RefundProcessed
  | PremiumContentUnlocked;

/**
 * All social-related events.
 */
export type SocialEvent =
  | ScenarioRated
  | ScenarioReviewed
  | ScenarioShared
  | UserFollowed
  | UserUnfollowed
  | CommentPosted
  | CommentDeleted
  | ContentLiked
  | ContentUnliked;

/**
 * All moderation-related events.
 */
export type ModerationEvent =
  | ContentReported
  | ContentModerated
  | UserWarned
  | UserSuspended
  | UserBanned
  | AppealSubmitted
  | AppealResolved;

/**
 * All Mystira domain events.
 */
export type MystiraEvent =
  | AccountEvent
  | SessionEvent
  | ContentEvent
  | CacheEvent
  | AIEvent
  | UserEvent
  | NotificationEvent
  | ProgressionEvent
  | GamificationEvent
  | PaymentEvent
  | SocialEvent
  | ModerationEvent;

/**
 * Event type discriminator values.
 */
export type EventType = MystiraEvent['type'];
